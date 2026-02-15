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
        Dictionary<string, Equipment> EquippedItems = new Dictionary<string, Equipment>();
        public PlayerEquipments()
        {

        }

        public void Equip(Equipment item)
        {
            if (!EquippedItems.ContainsKey(item.ItemName))
            {
                EquippedItems.Add(item.ItemName, item);
            }
        }

        public void Unequip(string itemName)
        {
            if (EquippedItems.ContainsKey(itemName))
            {
                EquippedItems.Remove(itemName);
            }
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
                {
                    ConsumableItems.RemoveAt(selectedIndex);
                }
            }
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
        public string gender { get; set; } = "Not Specified";
        public int age { get; set; }

        public string personality { get; set; } = "Neutral";

        public string job { get; set; } = "Adventurer";

        public string backgroundStory { get; set; } = "A mysterious past.";

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

        public TrpgPlayer(string name, string description = "", string className = "") : base(name, description)
        {
            playerClass = new PlayerClass(className);
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
    }

    
}
