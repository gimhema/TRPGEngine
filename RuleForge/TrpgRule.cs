using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RuleForge
{
    // ============================================================
    // 아이템 레지스트리 - ID 기반 아이템 인스턴스 조회
    // ============================================================
    public static class TrpgItemRegistry
    {
        private static readonly Dictionary<int, TrpgItem> Items = new();

        public static void Register(int id, TrpgItem item) => Items[id] = item;
        public static TrpgItem? Get(int id) => Items.TryGetValue(id, out var item) ? item : null;
        public static void Clear() => Items.Clear();

        /// <summary>이름으로 아이템 ID를 역조회한다. 없으면 -1 반환.</summary>
        public static int FindId(string itemName)
        {
            foreach (var kv in Items)
                if (kv.Value.ItemName == itemName) return kv.Key;
            return -1;
        }
    }

    // ============================================================
    // 적 레지스트리 - ID 기반 적 인스턴스 조회
    // ============================================================
    public static class TrpgEnemyRegistry
    {
        private static readonly Dictionary<int, TrpgEnemy> Enemies = new();

        public static void Register(int id, TrpgEnemy enemy) => Enemies[id] = enemy;
        public static TrpgEnemy? Get(int id) => Enemies.TryGetValue(id, out var e) ? e : null;
        public static void Clear() => Enemies.Clear();
    }

    // ============================================================
    // 퀘스트 레지스트리 - ID 기반 퀘스트 인스턴스 조회
    // ============================================================
    public static class TrpgQuestRegistry
    {
        private static readonly Dictionary<int, Quest> Quests = new();

        public static void Register(int id, Quest quest) => Quests[id] = quest;
        public static Quest? Get(int id) => Quests.TryGetValue(id, out var q) ? q : null;
        public static void Clear() => Quests.Clear();
        public static IEnumerable<Quest> All() => Quests.Values;
    }

    // ============================================================
    // JSON DTO (파싱 전용 내부 클래스)
    // ============================================================

    internal class SkillEffectDto
    {
        public string EffectType { get; set; } = "Damage";
        public int Value { get; set; }
        public string TargetStat { get; set; } = "";
    }

    internal class SkillDto
    {
        public string SkillName { get; set; } = "";
        public string Description { get; set; } = "";
        public int MpCost { get; set; }
        public string TargetType { get; set; } = "Enemy";
        public int RequiredLevel { get; set; }
        public List<SkillEffectDto> Effects { get; set; } = new();
    }

    internal class SkillFileDto
    {
        public List<SkillDto> StarterSkills { get; set; } = new();
        public List<SkillDto> AdvancedSkills { get; set; } = new();
    }

    internal class ItemStatDto
    {
        public int ATK { get; set; }
        public int DEF { get; set; }
    }

    internal class ItemEffectDto
    {
        public int HP { get; set; }
        public int MP { get; set; }
    }

    internal class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public int Price { get; set; }
        public ItemStatDto? Stat { get; set; }
        public ItemEffectDto? Effect { get; set; }
    }

    internal class ItemFileDto
    {
        public List<ItemDto> Items { get; set; } = new();
    }

    internal class EnemyDto
    {
        public int Id { get; set; }
        public string EnemyName { get; set; } = "";
        public int HP { get; set; }
        public int ATK { get; set; }
        public int DEF { get; set; }
        public int SPD { get; set; }
        public int ExpReward { get; set; } = 10;
        public int GoldReward { get; set; } = 0;
        public List<int> RewardItemIds { get; set; } = new();
    }

    internal class EnemyFileDto
    {
        public List<EnemyDto> Enemies { get; set; } = new();
    }

    internal class NpcDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "Normal";
        public string Background { get; set; } = "";
        public List<string> Traits { get; set; } = new();
        public List<int> TradeItems { get; set; } = new();
        public List<int> QuestIds { get; set; } = new();
    }

    internal class NpcFileDto
    {
        public List<NpcDto> NPCs { get; set; } = new();
    }

    internal class EstablishmentDto
    {
        public string EstablishmentName { get; set; } = "";
        public List<string> NPCNames { get; set; } = new();
    }

    internal class VillageDto
    {
        public string VillageName { get; set; } = "";
        public string VillageDescription { get; set; } = "";
        public List<EstablishmentDto> Establishments { get; set; } = new();
    }

    internal class VillageFileDto
    {
        public List<VillageDto> Villages { get; set; } = new();
    }

    internal class DungeonDto
    {
        public string DungeonName { get; set; } = "";
        public string DungeonDescription { get; set; } = "";
        public List<int> EncounterEnemyIds { get; set; } = new();
    }

    internal class DungeonFileDto
    {
        public List<DungeonDto> Dungeons { get; set; } = new();
    }

    internal class FieldDto
    {
        public string FieldName { get; set; } = "";
        public string FieldDescription { get; set; } = "";
        public List<string> Villages { get; set; } = new();
        public List<string> Dungeons { get; set; } = new();
        public List<int> GatherableItems { get; set; } = new();
    }

    internal class WorldDto
    {
        public string WorldName { get; set; } = "";
        public string WorldDescription { get; set; } = "";
        public List<FieldDto> Fields { get; set; } = new();
    }

    internal class WorldFileDto
    {
        public List<WorldDto> Worlds { get; set; } = new();
    }

    internal class QuestDto
    {
        public int QuestId { get; set; }
        public string QuestTitle { get; set; } = "";
        public string QuestDescription { get; set; } = "";
        public List<int> QuestRewardItemIds { get; set; } = new();
    }

    internal class QuestFileDto
    {
        public List<QuestDto> QuestInfo { get; set; } = new();
    }

    // ============================================================
    // RulebookParser - JSON 룰북 파일 파싱 및 게임 오브젝트 생성
    // ============================================================
    public class RulebookParser
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly string RuleBookDir =
            Path.Combine(AppContext.BaseDirectory, "RuleBook");

        private static T LoadJson<T>(string filename) where T : new()
        {
            var path = Path.Combine(RuleBookDir, filename);
            if (!File.Exists(path))
            {
                Console.WriteLine($"[RulebookParser] 경고: {filename} 파일을 찾을 수 없습니다. ({path})");
                return new T();
            }
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
        }

        // --------------------------------------------------------
        // 스킬 파싱
        // --------------------------------------------------------
        /// <summary>TR_2_Skill.json 파싱 후 TrpgSkillData에 등록</summary>
        public void ParseSkills()
        {
            var data = LoadJson<SkillFileDto>("TR_2_Skill.json");
            TrpgSkillData.Clear();

            foreach (var dto in data.StarterSkills)
                TrpgSkillData.RegisterStarterSkill(DtoToSkill(dto));

            foreach (var dto in data.AdvancedSkills)
                TrpgSkillData.RegisterAdvancedSkill(DtoToSkill(dto));

            Console.WriteLine($"[RulebookParser] 스킬 로드 완료: " +
                $"기본 {data.StarterSkills.Count}개, 상급 {data.AdvancedSkills.Count}개");
        }

        private static TrpgSkill DtoToSkill(SkillDto dto)
        {
            var targetType = Enum.TryParse<SkillTargetType>(dto.TargetType, out var t)
                ? t : SkillTargetType.Enemy;
            var skill = new TrpgSkill(dto.SkillName, dto.Description, dto.MpCost, targetType)
            {
                RequiredLevel = dto.RequiredLevel
            };
            foreach (var e in dto.Effects)
            {
                var effectType = Enum.TryParse<SkillEffectType>(e.EffectType, out var et)
                    ? et : SkillEffectType.Damage;
                skill.AddEffect(new SkillEffect(effectType, e.Value, e.TargetStat));
            }
            return skill;
        }

        // --------------------------------------------------------
        // 아이템 파싱
        // --------------------------------------------------------
        /// <summary>TR_3_Item.json 파싱 후 TrpgItemRegistry에 등록. 반드시 적 파싱 전에 호출.</summary>
        public void ParseItems()
        {
            var data = LoadJson<ItemFileDto>("TR_3_Item.json");
            TrpgItemRegistry.Clear();

            foreach (var dto in data.Items)
            {
                TrpgItem item = dto.Type switch
                {
                    "Equipment"  => new Equipment(dto.Name, dto.Description, dto.Price),
                    "Consumable" => new Consumable(dto.Name, dto.Description, dto.Price),
                    "KeyItem"    => new KeyItem(dto.Name, dto.Description, dto.Price),
                    _            => new TrpgItem(dto.Name, dto.Description, dto.Price)
                };

                // Equipment: 스탯 보너스 주입
                if (item is Equipment equipment && dto.Stat != null)
                {
                    if (dto.Stat.ATK != 0) equipment.Stat["ATK"] = dto.Stat.ATK;
                    if (dto.Stat.DEF != 0) equipment.Stat["DEF"] = dto.Stat.DEF;
                }

                // Consumable: HP/MP 효과 데이터 주입
                if (item is Consumable consumable && dto.Effect != null)
                {
                    consumable.HealHP = dto.Effect.HP;
                    consumable.RestoreMP = dto.Effect.MP;
                    consumable.Quantity = 1;
                }

                TrpgItemRegistry.Register(dto.Id, item);
            }

            Console.WriteLine($"[RulebookParser] 아이템 로드 완료: {data.Items.Count}개");
        }

        // --------------------------------------------------------
        // 적 파싱
        // --------------------------------------------------------
        /// <summary>TR_5_Enemy.json 파싱 후 TrpgEnemyRegistry에 등록. 아이템 파싱 후 호출.</summary>
        public void ParseEnemies()
        {
            var data = LoadJson<EnemyFileDto>("TR_5_Enemy.json");
            TrpgEnemyRegistry.Clear();

            foreach (var dto in data.Enemies)
            {
                var enemy = new TrpgEnemy(dto.EnemyName);
                enemy.InitCombatStats(dto.HP, dto.ATK, dto.DEF, dto.SPD);
                enemy.ExpReward = dto.ExpReward;
                enemy.GoldReward = dto.GoldReward;

                foreach (var itemId in dto.RewardItemIds)
                {
                    var item = TrpgItemRegistry.Get(itemId);
                    if (item != null) enemy.BattleReward.Add(item);
                }

                TrpgEnemyRegistry.Register(dto.Id, enemy);
            }

            Console.WriteLine($"[RulebookParser] 적 로드 완료: {data.Enemies.Count}개");
        }

        // --------------------------------------------------------
        // NPC 파싱
        // --------------------------------------------------------
        /// <summary>TR_4_NPC.json 파싱 후 이름 기반 딕셔너리 반환.
        /// ParseQuests() 호출 후 실행해야 TrpgQuestRegistry 참조 가능.</summary>
        public Dictionary<string, TrpgNPC> ParseNPCs()
        {
            var data = LoadJson<NpcFileDto>("TR_4_NPC.json");
            var result = new Dictionary<string, TrpgNPC>();

            foreach (var dto in data.NPCs)
            {
                var npc = new TrpgNPC();
                npc.Name = dto.Name;
                npc.Background = dto.Background;
                npc.Traits = dto.Traits;
                npc.NpcType = dto.Type;

                foreach (var itemId in dto.TradeItems)
                {
                    var item = TrpgItemRegistry.Get(itemId);
                    if (item != null) npc.TradeItems.Add(item);
                }

                foreach (var questId in dto.QuestIds)
                {
                    var quest = TrpgQuestRegistry.Get(questId);
                    if (quest != null) npc.Quests.Add(quest);
                }

                result[dto.Name] = npc;
            }

            Console.WriteLine($"[RulebookParser] NPC 로드 완료: {data.NPCs.Count}개");
            return result;
        }

        // --------------------------------------------------------
        // 퀘스트 파싱
        // --------------------------------------------------------
        /// <summary>TR_6_Quest.json 파싱 후 Quest 목록 반환 및 TrpgQuestRegistry 등록.
        /// ParseItems() 이후, ParseNPCs() 이전에 호출해야 한다.</summary>
        public List<Quest> ParseQuests()
        {
            var data = LoadJson<QuestFileDto>("TR_6_Quest.json");
            var result = new List<Quest>();

            foreach (var dto in data.QuestInfo)
            {
                var quest = new Quest(dto.QuestTitle, Quest.QuestType.Sub)
                {
                    QuestId = dto.QuestId,
                    Description = dto.QuestDescription
                };

                foreach (var itemId in dto.QuestRewardItemIds)
                {
                    var item = TrpgItemRegistry.Get(itemId);
                    if (item != null) quest.RewardItems.Add(item);
                }

                TrpgQuestRegistry.Register(dto.QuestId, quest);
                result.Add(quest);
            }

            Console.WriteLine($"[RulebookParser] 퀘스트 로드 완료: {data.QuestInfo.Count}개");
            return result;
        }

        // --------------------------------------------------------
        // 월드 파싱
        // --------------------------------------------------------
        /// <summary>TR_1_0/1/2 파일 파싱 후 WorldManager에 World 등록.
        /// 적 파싱 후 호출 필요 (던전 적 참조).</summary>
        public void ParseWorld(WorldManager worldMgr)
        {
            var worldData   = LoadJson<WorldFileDto>("TR_1_0_World.json");
            var villageData = LoadJson<VillageFileDto>("TR_1_1_Village.json");
            var dungeonData = LoadJson<DungeonFileDto>("TR_1_2_Dungeon.json");
            var npcMap      = ParseNPCs();

            // 마을 인스턴스 맵 생성
            var villageMap = new Dictionary<string, Village>();
            foreach (var vDto in villageData.Villages)
            {
                var village = new Village();
                village.SetWorldInfo(vDto.VillageName, vDto.VillageDescription);

                foreach (var estDto in vDto.Establishments)
                {
                    var est = new Village.Establishment { Name = estDto.EstablishmentName };
                    foreach (var npcName in estDto.NPCNames)
                    {
                        if (npcMap.TryGetValue(npcName, out var npc))
                            est.npcs[npcName] = npc;
                    }
                    village.establishments[estDto.EstablishmentName] = est;
                }

                villageMap[vDto.VillageName] = village;
            }

            // 던전 인스턴스 맵 생성
            var dungeonMap = new Dictionary<string, Dungeon>();
            foreach (var dDto in dungeonData.Dungeons)
            {
                var dungeon = new Dungeon();
                dungeon.SetWorldInfo(dDto.DungeonName, dDto.DungeonDescription);

                foreach (var enemyId in dDto.EncounterEnemyIds)
                {
                    var enemy = TrpgEnemyRegistry.Get(enemyId);
                    if (enemy != null) dungeon.EnemyList.Add(enemy);
                }

                dungeonMap[dDto.DungeonName] = dungeon;
            }

            // 월드 조립: World → Field → Village/Dungeon 연결
            foreach (var wDto in worldData.Worlds)
            {
                var world = new World();
                world.SetWorldInfo(wDto.WorldName, wDto.WorldDescription);
                world.units = new Dictionary<string, WorldUnit>();

                foreach (var fDto in wDto.Fields)
                {
                    var field = new Field();
                    field.SetWorldInfo(fDto.FieldName, fDto.FieldDescription);
                    field.GatherableItems.AddRange(
                        fDto.GatherableItems.Select(id => id.ToString()));

                    foreach (var vName in fDto.Villages)
                        if (villageMap.TryGetValue(vName, out var v))
                            field.AddConnection(v);

                    foreach (var dName in fDto.Dungeons)
                        if (dungeonMap.TryGetValue(dName, out var d))
                            field.AddConnection(d);

                    world.units[fDto.FieldName] = field;
                }

                worldMgr.Worlds[wDto.WorldName] = world;
            }

            Console.WriteLine($"[RulebookParser] 월드 로드 완료: " +
                $"{worldData.Worlds.Count}개 월드, " +
                $"{villageData.Villages.Count}개 마을, " +
                $"{dungeonData.Dungeons.Count}개 던전");
        }

        // --------------------------------------------------------
        // 개요(스토리) 파싱
        // --------------------------------------------------------
        /// <summary>TR_0_Overview.md 파싱 후 NarratorManager에 주입.</summary>
        public void ParseOverview()
        {
            var path = Path.Combine(RuleBookDir, "TR_0_Overview.md");
            if (!File.Exists(path))
            {
                Console.WriteLine("[RulebookParser] 경고: TR_0_Overview.md 파일을 찾을 수 없습니다.");
                return;
            }

            var lines = File.ReadAllLines(path);
            string mainStory = ExtractCodeBlock(lines, "# MainStory");
            var chapters = ParseChapterOverviews(lines);

            NarratorManager.Instance.LoadStory(mainStory, chapters);
            Console.WriteLine($"[RulebookParser] 개요 로드 완료: 챕터 {chapters.Count}개");
        }

        /// <summary>지정한 섹션 헤더 이후에 나오는 첫 ``` 코드 블록 내용을 반환.</summary>
        private static string ExtractCodeBlock(string[] lines, string sectionHeader)
        {
            bool inSection = false;
            bool inBlock = false;
            var sb = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                if (!inSection)
                {
                    if (line.TrimStart('#').Trim() == sectionHeader.TrimStart('#').Trim())
                        inSection = true;
                    continue;
                }

                // 다음 최상위 섹션 만나면 중단
                if (inSection && !inBlock && line.StartsWith("# ") &&
                    !line.TrimStart('#').Trim().Equals(sectionHeader.TrimStart('#').Trim()))
                    break;

                if (line.TrimStart() == "```")
                {
                    if (inBlock) break;   // 코드 블록 끝
                    inBlock = true;
                    continue;
                }

                if (inBlock)
                    sb.AppendLine(line);
            }

            return sb.ToString().Trim();
        }

        /// <summary>#### N / #### FINAL 챕터 섹션에서 ChapterOverview 목록 파싱.</summary>
        private static List<ChapterOverview> ParseChapterOverviews(string[] lines)
        {
            var result = new List<ChapterOverview>();
            ChapterOverview? current = null;
            bool inStoryBlock = false;
            var storySb = new System.Text.StringBuilder();

            void SaveCurrent()
            {
                if (current == null) return;
                current.Story = storySb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(current.Story) || !string.IsNullOrWhiteSpace(current.Goal))
                    result.Add(current);
                storySb.Clear();
            }

            foreach (var line in lines)
            {
                // #### FINAL 또는 #### N 감지
                if (line.StartsWith("#### "))
                {
                    SaveCurrent();
                    inStoryBlock = false;
                    var token = line.Substring(5).Trim();
                    current = new ChapterOverview();
                    if (token.Equals("FINAL", StringComparison.OrdinalIgnoreCase))
                    {
                        current.IsFinal = true;
                        current.ChapterNumber = 999;
                    }
                    else if (int.TryParse(token, out int num))
                    {
                        current.ChapterNumber = num;
                    }
                    continue;
                }

                if (current == null) continue;

                // 목표: 파싱
                if (line.TrimStart().StartsWith("목표:"))
                {
                    current.Goal = line.Substring(line.IndexOf("목표:") + 3).Trim();
                    continue;
                }

                // 스토리 코드 블록
                if (line.TrimStart() == "```")
                {
                    inStoryBlock = !inStoryBlock;
                    continue;
                }

                if (inStoryBlock)
                    storySb.AppendLine(line);
            }

            SaveCurrent();
            return result;
        }

        // --------------------------------------------------------
        // 전체 파싱 진입점
        // --------------------------------------------------------
        /// <summary>모든 룰북 파일 파싱. 순서: Items → Enemies → Skills → Quests → World(+NPCs) → Overview</summary>
        public List<Quest> ParseAll(WorldManager worldMgr)
        {
            Console.WriteLine("[RulebookParser] 룰북 파싱 시작...");
            ParseItems();          // 아이템 먼저 (적/퀘스트 보상 참조)
            ParseEnemies();        // 던전 조립 전에
            ParseSkills();
            var quests = ParseQuests();  // NPC 퀘스트 링크 전에 레지스트리 등록
            ParseWorld(worldMgr);        // 내부에서 ParseNPCs() 호출 (퀘스트 레지스트리 참조)
            ParseOverview();             // 스토리/챕터 개요 → NarratorManager 주입
            Console.WriteLine("[RulebookParser] 룰북 파싱 완료.");
            return quests;
        }
    }

    // ============================================================
    // TrpgRule (기존 액션 시스템, 유지)
    // ============================================================
    class TrpgRule
    {
        private Dictionary<string, TrpgGameAction> GameActions = new();

        public void InitializeGameActionsByRulebook(string filePath) { }

        public TrpgRule() { }

        public void DoAction(string actionName)
        {
            if (GameActions.ContainsKey(actionName))
                GameActions[actionName].ExecuteAction();
            else
                Console.WriteLine($"Action '{actionName}' not found.");
        }
    }
}
