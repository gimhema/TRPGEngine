using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    public class PlayerClass
    {
        public string ClassName { get; set; }

        public PlayerClass(string className)
        {
            ClassName = className;
        }
    }

// 장비창
    public class PlayerEquipments
    {
        public Dictionary<string, Equipment> EquippedItems { get; } = new Dictionary<string, Equipment>();

        public void Equip(Equipment item)
        {
            if (!EquippedItems.ContainsKey(item.ItemName))
                EquippedItems.Add(item.ItemName, item);
        }

        public void Unequip(string itemName)
        {
            EquippedItems.Remove(itemName);
        }

        public bool IsEquipped(string itemName) => EquippedItems.ContainsKey(itemName);

        /// <summary>
        /// 장착된 모든 장비에서 특정 스탯의 합산 보너스를 반환한다.
        /// </summary>
        public int GetTotalBonus(string statName)
        {
            int total = 0;
            foreach (var eq in EquippedItems.Values)
                total += eq.GetStatBonus(statName);
            return total;
        }
    }

    public class PlayerItemBag
    {
        public List<Consumable> ConsumableItems { get; set; }
        public List<Equipment> EquipmentItems { get; set; }

        public List<KeyItem> KeyItems { get; set; }

        public PlayerItemBag()
        {
            ConsumableItems = new List<Consumable>();
            EquipmentItems = new List<Equipment>();
            KeyItems = new List<KeyItem>();
        }

// 장비 아이템 전용
        public void AcquireEquipment(Equipment item)
        {
            EquipmentItems.Add(item);
        }
        public void DropEquipment(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < EquipmentItems.Count)
            {
                EquipmentItems.RemoveAt(selectedIndex);
            }
        }
// 장비 장착
        public void EquipItem(int selectedIndex, PlayerEquipments equipments)
        {
            if (selectedIndex >= 0 && selectedIndex < EquipmentItems.Count)
            {
                Equipment itemToEquip = EquipmentItems[selectedIndex];
                equipments.Equip(itemToEquip);
            }
        }

        public void UnequipItem(string itemName, PlayerEquipments equipments)
        {
            equipments.Unequip(itemName);
        }


// 소비 아이템 전용
        public void AcquireConsumable(Consumable item)
        {
            ConsumableItems.Add(item);
        }
        public void UseConsumable(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < ConsumableItems.Count)
            {
                ConsumableItems[selectedIndex].Use();
                if (ConsumableItems[selectedIndex].Quantity <= 0)
                    ConsumableItems.RemoveAt(selectedIndex);
            }
        }

        /// <summary>
        /// 소비 아이템을 사용하고 target에 효과를 적용한다. 효과 로그 목록을 반환한다.
        /// </summary>
        public List<string> UseConsumable(int selectedIndex, TrpgActor target)
        {
            if (selectedIndex < 0 || selectedIndex >= ConsumableItems.Count)
                return new List<string>();

            var item = ConsumableItems[selectedIndex];
            var logs = item.ApplyEffect(target);
            item.Use();
            if (item.Quantity <= 0)
                ConsumableItems.RemoveAt(selectedIndex);
            return logs;
        }

        public void DropConsumable(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < ConsumableItems.Count)
            {
                ConsumableItems.RemoveAt(selectedIndex);
            }
        }

// 키 아이템 전용
        public void SubmitKeyItem(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < KeyItems.Count)
            {
                KeyItems[selectedIndex].Use();
                KeyItems.RemoveAt(selectedIndex);
            }
        }
    }

    public class PlayerProfile
    {
        public string gender { get; set; } = "";
        public int age { get; set; }
        public string personality { get; set; } = "";
        public string job { get; set; } = "";
        public string backgroundStory { get; set; } = "";
        public int PlayerLevel { get; set; }

        public PlayerProfile()
        {
        }
    }

    public class TrpgPlayer : TrpgActor
    {
        public PlayerProfile playerProfile = new PlayerProfile();
        public PlayerClass playerClass { get; set; }
        public PlayerItemBag playerItemBag { get; set; } = new PlayerItemBag();
        public PlayerEquipments playerEquipments { get; set; } = new PlayerEquipments();
        public List<TrpgSkill> PlayerSkills { get; set; } = new List<TrpgSkill>();

        /// <summary>플레이어 소지금</summary>
        public int Gold { get; set; }

        /// <summary>수락한 퀘스트 목록</summary>
        public List<Quest> AcceptedQuests { get; set; } = new();

        /// <summary>현재 경험치</summary>
        public int Exp { get; set; }

        /// <summary>다음 레벨까지 필요한 경험치 (레벨 × 100)</summary>
        public int ExpToNextLevel { get; set; }

        public TrpgPlayer(string name, string description = "", string className = "") : base(name, description)
        {
            playerClass = new PlayerClass(className);
            Gold = 0;
            Exp = 0;
            ExpToNextLevel = 100;
        }

        /// <summary>
        /// 스킬을 습득한다.
        /// </summary>
        public bool LearnSkill(TrpgSkill skill)
        {
            if (PlayerSkills.Exists(s => s.SkillName == skill.SkillName))
                return false;

            if (skill.RequiredLevel > playerProfile.PlayerLevel)
                return false;

            PlayerSkills.Add(skill);
            return true;
        }

        /// <summary>
        /// 스킬을 잊는다.
        /// </summary>
        public bool ForgetSkill(string skillName)
        {
            return PlayerSkills.RemoveAll(s => s.SkillName == skillName) > 0;
        }

        /// <summary>
        /// 현재 사용 가능한 스킬 목록 (MP가 충분한 스킬)
        /// </summary>
        public List<TrpgSkill> GetUsableSkills()
        {
            return PlayerSkills.FindAll(s => s.CanUse(this));
        }

        /// <summary>
        /// 경험치를 획득하고 레벨업 조건을 확인한다. 결과 로그 목록을 반환한다.
        /// </summary>
        public List<string> GainExp(int exp)
        {
            var logs = new List<string>();
            if (exp <= 0) return logs;

            Exp += exp;
            logs.Add($"EXP +{exp} ({Exp}/{ExpToNextLevel})");

            while (Exp >= ExpToNextLevel)
            {
                Exp -= ExpToNextLevel;
                LevelUp(logs);
            }
            return logs;
        }

        private void LevelUp(List<string> logs)
        {
            playerProfile.PlayerLevel++;
            ExpToNextLevel = playerProfile.PlayerLevel * 100;

            // 기본 스탯 증가
            var increases = new Dictionary<string, int>
            {
                { "HP", 10 }, { "MP", 5 }, { "ATK", 2 }, { "DEF", 1 }, { "SPD", 1 }
            };
            foreach (var kv in increases)
            {
                var stat = CommonAttributes.GetStatus(kv.Key);
                if (stat != null)
                    CommonAttributes.UpdateStatus(kv.Key, stat.StatusValue + kv.Value);
            }

            logs.Add($"레벨업! Lv.{playerProfile.PlayerLevel}");

            // 새로 습득 가능한 상급 스킬 확인
            foreach (var skill in TrpgSkillData.GetAdvancedSkills())
            {
                if (LearnSkill(skill))
                    logs.Add($"새 스킬 습득: [{skill.SkillName}]!");
            }
        }
    }

    
}
