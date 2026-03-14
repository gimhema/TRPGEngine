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
    // 저장/로드 관리자
    // ============================================================

    public static class GameSaveManager
    {
        private const string SaveFilePath = "save.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>세이브 파일 존재 여부</summary>
        public static bool HasSaveData() => File.Exists(SaveFilePath);

        // ─── 저장 ───

        /// <summary>
        /// 현재 게임 상태를 save.json에 저장한다.
        /// </summary>
        public static bool Save(TrpgGameState state, WorldManager worldMgr)
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

                File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(saveData, JsonOptions));
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
        /// save.json을 읽어 게임 상태를 복원한다.
        /// 룰북은 이미 파싱(TrpgGameLogic.LoadRulebook())된 상태여야 한다.
        /// </summary>
        public static bool Load(TrpgGameState state, WorldManager worldMgr)
        {
            try
            {
                if (!HasSaveData()) return false;

                var json = File.ReadAllText(SaveFilePath);
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

                // 위치를 찾지 못하면 탐험 씬만 전환
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

            // 장비
            foreach (var itemData in data.EquipmentItems)
            {
                var template = TrpgItemRegistry.Get(itemData.ItemId) as Equipment;
                if (template == null) continue;
                player.playerItemBag.AcquireEquipment(
                    new Equipment(template.ItemName, template.ItemDescription, template.Price));
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
