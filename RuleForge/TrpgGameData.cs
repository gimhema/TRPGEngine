using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RuleForge
{
    // ============================================================
    // 세이브 데이터 DTO (JSON 직렬화 전용)
    // ============================================================

    public class ItemSaveData
    {
        public int ItemId { get; set; } = -1;
        public string ItemName { get; set; } = "";
        public string ItemType { get; set; } = "";   // "Consumable" | "Equipment" | "KeyItem"
        public int Quantity { get; set; } = 1;
    }

    public class QuestSaveData
    {
        public int QuestId { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class PlayerSaveData
    {
        public string Name { get; set; } = "";
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int ExpToNextLevel { get; set; } = 100;
        public int Gold { get; set; } = 0;
        public Dictionary<string, int> Stats { get; set; } = new();
        public List<ItemSaveData> ConsumableItems { get; set; } = new();
        public List<ItemSaveData> EquipmentItems { get; set; } = new();
        public List<ItemSaveData> KeyItems { get; set; } = new();
        public List<string> SkillNames { get; set; } = new();
        /// <summary>장착 중인 장비 이름 목록</summary>
        public List<string> EquippedItemNames { get; set; } = new();
    }

    public class GameSaveData
    {
        public int SaveVersion { get; set; } = 1;
        public string SaveTime { get; set; } = "";
        public PlayerSaveData? Player { get; set; }
        public string CurrentLocationId { get; set; } = "";
        public Dictionary<string, bool> DungeonClearStates { get; set; } = new();
        public List<QuestSaveData> QuestStates { get; set; } = new();
    }

    // ============================================================
    // 세이브 슬롯 요약 정보
    // ============================================================

    public class SaveSlotInfo
    {
        public int Slot { get; set; }
        public bool HasData { get; set; }
        public string SaveTime { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public int Level { get; set; }
        public string LocationId { get; set; } = "";

        public string DisplayText()
        {
            if (!HasData) return "(빈 슬롯)";
            return $"{PlayerName} Lv.{Level}  [{SaveTime}]";
        }
    }

    // ============================================================
    // 저장/로드 관리자
    // ============================================================

    public static class GameSaveManager
    {
        public const int SlotCount = 3;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static string SlotPath(int slot) => $"save_{slot}.json";

        // ─── 슬롯 정보 조회 ───

        public static SaveSlotInfo GetSlotInfo(int slot)
        {
            var info = new SaveSlotInfo { Slot = slot };
            string path = SlotPath(slot);

            if (!File.Exists(path))
            {
                info.HasData = false;
                return info;
            }

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<GameSaveData>(json, JsonOptions);
                if (data == null) { info.HasData = false; return info; }

                info.HasData = true;
                info.SaveTime = data.SaveTime;
                info.LocationId = data.CurrentLocationId;
                if (data.Player != null)
                {
                    info.PlayerName = data.Player.Name;
                    info.Level = data.Player.Level;
                }
            }
            catch
            {
                info.HasData = false;
            }

            return info;
        }

        /// <summary>저장된 슬롯이 하나라도 있으면 true</summary>
        public static bool HasAnySaveData()
        {
            for (int i = 1; i <= SlotCount; i++)
                if (File.Exists(SlotPath(i))) return true;
            return false;
        }

        // ─── 저장 ───

        /// <summary>
        /// 지정 슬롯에 현재 게임 상태를 저장한다.
        /// </summary>
        public static bool Save(TrpgGameState state, WorldManager worldMgr, int slot)
        {
            try
            {
                var saveData = new GameSaveData
                {
                    SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    CurrentLocationId = state.CurrentLocation?.Id ?? ""
                };

                if (state.CurrentPlayer != null)
                    saveData.Player = SerializePlayer(state.CurrentPlayer);

                // 던전 클리어 상태 수집
                foreach (var world in worldMgr.Worlds.Values)
                {
                    if (world.units == null) continue;
                    foreach (var unit in world.units.Values)
                    {
                        if (unit is Dungeon dungeon)
                            saveData.DungeonClearStates[dungeon.Id] = dungeon.IsClear;
                    }
                }

                // 퀘스트 상태 수집
                foreach (var quest in TrpgQuestRegistry.All())
                {
                    if (quest.IsAccepted || quest.IsCompleted)
                    {
                        saveData.QuestStates.Add(new QuestSaveData
                        {
                            QuestId = quest.QuestId,
                            IsAccepted = quest.IsAccepted,
                            IsCompleted = quest.IsCompleted
                        });
                    }
                }

                File.WriteAllText(SlotPath(slot), JsonSerializer.Serialize(saveData, JsonOptions));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[저장 오류] {ex.Message}");
                return false;
            }
        }

        // ─── 불러오기 ───

        /// <summary>
        /// 지정 슬롯을 읽어 게임 상태를 복원한다.
        /// 룰북은 이미 파싱된 상태여야 한다.
        /// </summary>
        public static bool Load(TrpgGameState state, WorldManager worldMgr, int slot)
        {
            try
            {
                string path = SlotPath(slot);
                if (!File.Exists(path)) return false;

                var json = File.ReadAllText(path);
                var saveData = JsonSerializer.Deserialize<GameSaveData>(json, JsonOptions);
                if (saveData == null) return false;

                // 플레이어 복원
                if (saveData.Player != null)
                    state.CurrentPlayer = DeserializePlayer(saveData.Player);

                // 던전 클리어 상태 복원
                foreach (var world in worldMgr.Worlds.Values)
                {
                    if (world.units == null) continue;
                    foreach (var unit in world.units.Values)
                    {
                        if (unit is Dungeon dungeon &&
                            saveData.DungeonClearStates.TryGetValue(dungeon.Id, out bool isClear))
                        {
                            dungeon.IsClear = isClear;
                        }
                    }
                }

                // 퀘스트 상태 복원
                foreach (var qs in saveData.QuestStates)
                {
                    var quest = TrpgQuestRegistry.Get(qs.QuestId);
                    if (quest == null) continue;

                    quest.IsAccepted = qs.IsAccepted;
                    quest.IsCompleted = qs.IsCompleted;

                    if (qs.IsAccepted && state.CurrentPlayer != null &&
                        !state.CurrentPlayer.AcceptedQuests.Contains(quest))
                    {
                        state.CurrentPlayer.AcceptedQuests.Add(quest);
                    }
                }

                // 저장 시점 위치로 이동
                if (!string.IsNullOrEmpty(saveData.CurrentLocationId))
                {
                    var unit = FindUnitById(worldMgr, saveData.CurrentLocationId);
                    if (unit != null)
                    {
                        state.ChangeScene(TrpgGameState.SceneType.Exploration);
                        unit.Action(state);
                        return true;
                    }
                }

                state.ChangeScene(TrpgGameState.SceneType.Exploration);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[불러오기 오류] {ex.Message}");
                return false;
            }
        }

        // ─── 직렬화 헬퍼 ───

        private static PlayerSaveData SerializePlayer(TrpgPlayer player)
        {
            var data = new PlayerSaveData
            {
                Name = player.Name,
                Level = player.playerProfile.PlayerLevel,
                Exp = player.Exp,
                ExpToNextLevel = player.ExpToNextLevel,
                Gold = player.Gold
            };

            foreach (var statName in new[] { "HP", "MP", "ATK", "DEF", "SPD" })
            {
                var stat = player.CommonAttributes.GetStatus(statName);
                if (stat != null) data.Stats[statName] = stat.StatusValue;
            }

            foreach (var item in player.playerItemBag.ConsumableItems)
                data.ConsumableItems.Add(new ItemSaveData
                {
                    ItemId = TrpgItemRegistry.FindId(item.ItemName),
                    ItemName = item.ItemName,
                    ItemType = "Consumable",
                    Quantity = item.Quantity
                });

            foreach (var item in player.playerItemBag.EquipmentItems)
                data.EquipmentItems.Add(new ItemSaveData
                {
                    ItemId = TrpgItemRegistry.FindId(item.ItemName),
                    ItemName = item.ItemName,
                    ItemType = "Equipment"
                });

            foreach (var item in player.playerItemBag.KeyItems)
                data.KeyItems.Add(new ItemSaveData
                {
                    ItemId = TrpgItemRegistry.FindId(item.ItemName),
                    ItemName = item.ItemName,
                    ItemType = "KeyItem"
                });

            data.SkillNames = player.PlayerSkills.Select(s => s.SkillName).ToList();

            // 장착 중인 장비 이름 저장
            data.EquippedItemNames = player.playerEquipments.EquippedItems.Keys.ToList();

            return data;
        }

        private static TrpgPlayer DeserializePlayer(PlayerSaveData data)
        {
            var player = new TrpgPlayer(data.Name);
            player.playerProfile.PlayerLevel = data.Level;
            player.Exp = data.Exp;
            player.ExpToNextLevel = data.ExpToNextLevel;
            player.Gold = data.Gold;

            foreach (var kv in data.Stats)
                player.CommonAttributes.AddNewStatus(kv.Key, kv.Value);

            // 소비 아이템
            foreach (var itemData in data.ConsumableItems)
            {
                var template = TrpgItemRegistry.Get(itemData.ItemId) as Consumable;
                if (template == null) continue;

                player.playerItemBag.AcquireConsumable(new Consumable(
                    template.ItemName, template.ItemDescription, template.Price)
                {
                    HealHP = template.HealHP,
                    RestoreMP = template.RestoreMP,
                    Quantity = itemData.Quantity
                });
            }

            // 장비 (가방에 추가 + 장착 복원)
            var equippedSet = new HashSet<string>(data.EquippedItemNames);
            foreach (var itemData in data.EquipmentItems)
            {
                var template = TrpgItemRegistry.Get(itemData.ItemId) as Equipment;
                if (template == null) continue;

                var eq = new Equipment(template.ItemName, template.ItemDescription, template.Price);
                foreach (var kv in template.Stat)
                    eq.Stat[kv.Key] = kv.Value;

                player.playerItemBag.AcquireEquipment(eq);

                if (equippedSet.Contains(eq.ItemName))
                    player.playerEquipments.Equip(eq);
            }

            // 키 아이템
            foreach (var itemData in data.KeyItems)
            {
                var template = TrpgItemRegistry.Get(itemData.ItemId) as KeyItem;
                if (template == null) continue;
                player.playerItemBag.KeyItems.Add(
                    new KeyItem(template.ItemName, template.ItemDescription, template.Price));
            }

            // 스킬 (레벨 체크 없이 직접 복원)
            var allSkills = TrpgSkillData.GetStarterSkills()
                .Concat(TrpgSkillData.GetAdvancedSkills());
            foreach (var skillName in data.SkillNames)
            {
                var skill = allSkills.FirstOrDefault(s => s.SkillName == skillName);
                if (skill != null && !player.PlayerSkills.Exists(s => s.SkillName == skillName))
                    player.PlayerSkills.Add(skill);
            }

            return player;
        }

        private static WorldUnit? FindUnitById(WorldManager worldMgr, string id)
        {
            foreach (var world in worldMgr.Worlds.Values)
            {
                if (world.units == null) continue;
                foreach (var unit in world.units.Values)
                {
                    if (unit.Id == id) return unit;
                }
            }
            return null;
        }
    }
}
