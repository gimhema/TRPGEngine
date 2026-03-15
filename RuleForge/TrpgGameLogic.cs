using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    // Chapter
    public class Chapter
    {
        public string Title { get; set; }
        public List<Quest> Quests { get; set; }

        public int currentQuestIndex = 0;

        public Chapter(string title)
        {
            Title = title;
            Quests = new List<Quest>();
        }

        public string GetStartNarrative()
        {
            var narrative = $"Chapter started: {Title}\n";
            foreach (var quest in Quests)
            {
                narrative += $"Starting quest: {quest.Title} ({quest.Type})\n";
            }
            return narrative;
        }

        public bool SelectQuest(int index)
        {
            if (index >= 0 && index < Quests.Count)
            {
                currentQuestIndex = index;
                return true;
            }
            return false;
        }

        public bool IsCanNextChapter()
        {
            bool _ret = false;

            // 메인 퀘스트가 클리어되었다면 _ret을 true로 변경
            if (Quests.Where(q => q.Type == Quest.QuestType.Main).All(q => q.IsCompleted))
            {
                _ret = true;
            }

            return _ret;
        }
    }

    // Quest
    public class Quest
    {
        public enum QuestType
        {
            Main,
            Sub
        }

        public int QuestId { get; set; } = -1;
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestType Type { get; set; }

        public bool IsAccepted { get; set; } = false;
        public bool IsCompleted { get; set; } = false;

        /// <summary>퀘스트 완료 보상 아이템 목록 (룰북에서 주입)</summary>
        public List<TrpgItem> RewardItems { get; set; } = new();

        public Quest(string title, QuestType type)
        {
            Title = title;
            Type = type;
            Description = "";
        }

        public string GetNarrative()
        {
            return $"[{Title}]\n{Description}";
        }

    }

    // Activity
    public class Activity
    {
        public enum ActivityType
        {
            Combat,
            Exploration,
            Social
        }

        public ActivityType Type { get; set; }

        public Activity(ActivityType type)
        {
            Type = type;
        }

        public string GetActivityNarrative()
        {
            switch(Type)
            {
                case ActivityType.Combat:
                    return "Engaging in combat...";
                case ActivityType.Exploration:
                    return "Exploring the area...";
                case ActivityType.Social:
                    return "Interacting with NPCs...";
                default:
                    return $"Activity: {Type}";
            }
        }

    }
        

    public class TrpgGameLogic
    {
        private static TrpgGameLogic? _instance;
        private TrpgRule GameRule = new TrpgRule();
        private List<Chapter> Chapters = new List<Chapter>();
        private WorldManager WorldMgr = new WorldManager();

        private Dictionary<string, TrpgPlayer> Players = new Dictionary<string, TrpgPlayer>();

        private bool IsGameExit { get; set; } = false;

        private bool IsGameCleard { get; set; } = false;

        public void SetGameExit(bool isExit)
        {
            IsGameExit = isExit;
        }

        public void SetGameCleard(bool isCleard)
        {
            IsGameCleard = isCleard;
        }

        public void LoadChapters(List<Chapter> chapters)
        {
            Chapters = chapters;
        }

        /// <summary>저장/로드 시스템에서 WorldManager에 접근하기 위한 프로퍼티</summary>
        public WorldManager WorldManager => WorldMgr;

        public static TrpgGameLogic Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TrpgGameLogic();
                }
                return _instance;
            }
        }

        public TrpgGameLogic()
        {
            // Initialize chapters and quests here if needed

        }

        /// <summary>
        /// 룰북 JSON 파일을 파싱하고 게임 시스템(스킬, 아이템, 적, 월드)을 초기화한다.
        /// GameStartPreprocess()에서 호출.
        /// </summary>
        public void LoadRulebook()
        {
            var parser = new RulebookParser();
            var quests = parser.ParseAll(WorldMgr);

            // 파싱된 퀘스트로 기본 챕터 생성
            if (quests.Count > 0)
            {
                var chapter = new Chapter("제1장: 모험의 시작");
                foreach (var quest in quests)
                    chapter.Quests.Add(quest);
                Chapters.Add(chapter);
            }
        }

        /// <summary>
        /// 첫 번째 Field(시작 위치)를 반환한다.
        /// </summary>
        public WorldUnit? GetStartingLocation()
        {
            foreach (var world in WorldMgr.Worlds.Values)
            {
                if (world.units == null) continue;
                foreach (var unit in world.units.Values)
                {
                    if (unit is Field) return unit;
                }
            }
            return null;
        }

        /// <summary>
        /// 플레이어 생성 후 시작 위치로 이동. PlayerSetup 씬의 선택지 콜백에서 호출.
        /// </summary>
        private void StartGame(string playerName, TrpgGameState state)
        {
            var player = CreatePlayer(playerName);
            state.CurrentPlayer = player;

            if (Chapters.Count > 0)
                state.CurrentChapter = Chapters[0];

            var startLocation = GetStartingLocation();
            if (startLocation != null)
            {
                state.ChangeScene(TrpgGameState.SceneType.Exploration);
                startLocation.Action(state);
            }
            else
            {
                state.ChangeScene(TrpgGameState.SceneType.Exploration);
                state.NarrativeText = $"환영합니다, {playerName}!\n월드 데이터가 없습니다. 룰북을 확인하세요.";
                state.ClearChoices();
            }
        }

        /// <summary>
        /// 게임 시작 (GameState 초기화). 플레이어 설정 화면의 선택지를 구성한다.
        /// </summary>
        public void InitializeGame(TrpgGameState state)
        {
            state.NarrativeText = "게임에 오신 것을 환영합니다!\n\n캐릭터를 생성하세요.";
            state.CurrentScene = TrpgGameState.SceneType.PlayerSetup;
            state.ClearChoices();

            state.AddChoice(new TrpgChoice("1", "1. 이름 입력하기")
            {
                OnSelect = (s) =>
                {
                    s.NarrativeText = "캐릭터 이름을 입력해주세요:";
                    s.ClearChoices();
                    s.SetCustomData("awaitingPlayerName", true);
                }
            });
            state.AddChoice(new TrpgChoice("2", "2. 기본 이름으로 시작 (모험가)")
            {
                OnSelect = (s) => StartGame("모험가", s)
            });

            // 세이브 파일이 있을 때만 불러오기 표시
            if (GameSaveManager.HasSaveData())
            {
                state.AddChoice(new TrpgChoice("3", "3. 이어하기")
                {
                    OnSelect = (s) =>
                    {
                        bool ok = GameSaveManager.Load(s, WorldMgr);
                        if (!ok)
                        {
                            s.NarrativeText = "세이브 파일을 불러오지 못했습니다.";
                            s.ClearChoices();
                            s.AddChoice(new TrpgChoice("1", "1. 처음으로")
                            {
                                OnSelect = (ns) => InitializeGame(ns)
                            });
                        }
                    }
                });
            }
        }

        public void DoAction(string actionName)
        {
            GameRule.DoAction(actionName);
        }

        /// <summary>
        /// 입력 처리 (GameController에서 호출). 선택지로 처리 안 된 자유 입력을 받는다.
        /// </summary>
        public void ProcessInput(string input, TrpgGameState state)
        {
            switch (state.CurrentScene)
            {
                case TrpgGameState.SceneType.PlayerSetup:
                    // 이름 입력 대기 중이면 입력값을 이름으로 사용
                    bool awaitingName = state.GetCustomData<bool>("awaitingPlayerName");
                    if (awaitingName && !string.IsNullOrWhiteSpace(input))
                    {
                        state.CustomData.Remove("awaitingPlayerName");
                        StartGame(input.Trim(), state);
                    }
                    break;
                case TrpgGameState.SceneType.Exploration:
                    bool dialogueActive = state.GetCustomData<bool>("npc_dialogue_active");
                    if (dialogueActive)
                        NpcDialogueManager.Instance.ProcessDialogueInput(input, state);
                    break;
                case TrpgGameState.SceneType.Combat:
                    break;
                default:
                    state.NarrativeText = $"처리되지 않은 입력: {input}";
                    break;
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        public TrpgPlayer CreatePlayer(string name, int age = 0, string gender = "",
            string personality = "", string job = "", string backgroundStory = "")
        {
            var cfg = TrpgGameConfig.PlayerDefault;
            var newPlayer = new TrpgPlayer(name);
            newPlayer.playerProfile.age = age > 0 ? age : 18;
            newPlayer.playerProfile.gender = !string.IsNullOrEmpty(gender) ? gender : cfg.DefaultGender;
            newPlayer.playerProfile.personality = !string.IsNullOrEmpty(personality) ? personality : cfg.DefaultPersonality;
            newPlayer.playerProfile.job = !string.IsNullOrEmpty(job) ? job : cfg.DefaultJob;
            newPlayer.playerProfile.backgroundStory = !string.IsNullOrEmpty(backgroundStory) ? backgroundStory : cfg.DefaultBackgroundStory;
            newPlayer.playerProfile.PlayerLevel = cfg.DefaultLevel;

            // 룰북에서 로드된 초기 스탯 적용
            // TODO: RulebookParser에서 PlayerDefaultConfig.InitialStats를 설정
            foreach (var stat in cfg.InitialStats)
            {
                newPlayer.CommonAttributes.AddNewStatus(stat.Key, stat.Value);
            }

            // 기본 스킬 습득
            foreach (var skill in TrpgSkillData.GetStarterSkills())
            {
                newPlayer.LearnSkill(skill);
            }

            Players[name] = newPlayer;
            return newPlayer;
        }

        /// <summary>
        /// 플레이어 가져오기
        /// </summary>
        public TrpgPlayer? GetPlayer(string name)
        {
            return Players.ContainsKey(name) ? Players[name] : null;
        }

        /// <summary>
        /// 챕터 시작
        /// </summary>
        public void StartChapter(int chapterIndex, TrpgGameState state)
        {
            if (chapterIndex >= 0 && chapterIndex < Chapters.Count)
            {
                state.CurrentChapter = Chapters[chapterIndex];
                state.NarrativeText = Chapters[chapterIndex].GetStartNarrative();
                state.ChangeScene(TrpgGameState.SceneType.Exploration);
            }
        }

        /// <summary>
        /// 월드 로드
        /// </summary>
        public void LoadWorld(string name, World world)
        {
            WorldMgr.Worlds[name] = world;
        }

        /// <summary>
        /// 월드 가져오기
        /// </summary>
        public World? GetWorld(string name)
        {
            return WorldMgr.SelectWorld(name);
        }

        /// <summary>
        /// 플레이어를 특정 위치로 진입시킨다.
        /// 씬을 Exploration으로 전환하고 해당 WorldUnit의 Action()을 호출한다.
        /// </summary>
        public void EnterLocation(WorldUnit location, TrpgGameState state)
        {
            state.ChangeScene(TrpgGameState.SceneType.Exploration);
            location.Action(state);
        }

        /// <summary>
        /// 인벤토리 씬을 열어 소비 아이템을 사용할 수 있게 한다.
        /// onReturn: 인벤토리를 닫을 때 호출할 콜백 (이전 화면 복귀용)
        /// </summary>
        public static void OpenInventory(TrpgGameState state, Action<TrpgGameState> onReturn)
        {
            state.ChangeScene(TrpgGameState.SceneType.Inventory);
            state.NarrativeText = "인벤토리를 열었습니다.";
            state.ClearChoices();

            state.AddChoice(new TrpgChoice("1", "1. 아이템 사용")
            {
                OnSelect = (s) => ShowUseItemMenu(s, onReturn)
            });
            state.AddChoice(new TrpgChoice("2", "2. 닫기")
            {
                OnSelect = (s) => onReturn(s)
            });
        }

        /// <summary>
        /// 소비 아이템 목록을 선택지로 표시하고, 선택 시 효과를 플레이어에게 적용한다.
        /// </summary>
        private static void ShowUseItemMenu(TrpgGameState state, Action<TrpgGameState> onReturn)
        {
            if (state.CurrentPlayer == null) { onReturn(state); return; }

            var consumables = state.CurrentPlayer.playerItemBag.ConsumableItems;

            state.ClearChoices();

            if (consumables.Count == 0)
            {
                state.NarrativeText = "사용할 수 있는 소비 아이템이 없습니다.";
                state.AddChoice(new TrpgChoice("1", "1. 돌아가기")
                {
                    OnSelect = (s) => OpenInventory(s, onReturn)
                });
                return;
            }

            state.NarrativeText = "사용할 아이템을 선택하세요.";

            int index = 1;
            for (int i = 0; i < consumables.Count; i++)
            {
                var item = consumables[i];
                int itemIndex = i;

                var effectInfo = new System.Text.StringBuilder();
                if (item.HealHP > 0) effectInfo.Append($" HP+{item.HealHP}");
                if (item.RestoreMP > 0) effectInfo.Append($" MP+{item.RestoreMP}");

                state.AddChoice(new TrpgChoice(index.ToString(),
                    $"{index}. {item.ItemName} (x{item.Quantity}){effectInfo}")
                {
                    OnSelect = (s) =>
                    {
                        if (s.CurrentPlayer != null)
                        {
                            var logs = s.CurrentPlayer.playerItemBag.UseConsumable(itemIndex, s.CurrentPlayer);
                            s.NarrativeText = logs.Count > 0
                                ? string.Join("\n", logs)
                                : "아이템을 사용했습니다.";
                        }
                        // 사용 후 인벤토리 화면으로 돌아가기 (목록 갱신)
                        OpenInventory(s, onReturn);
                    }
                });
                index++;
            }

            state.AddChoice(new TrpgChoice(index.ToString(), $"{index}. 돌아가기")
            {
                OnSelect = (s) => OpenInventory(s, onReturn)
            });
        }

        /// <summary>
        /// 전투를 시작한다.
        /// Combat 씬으로 전환하고 TrpgBattle을 생성하여 전투 루프를 개시한다.
        /// </summary>
        public void StartBattle(TrpgEnemy enemy, TrpgGameState state, Action<TrpgGameState, bool>? onBattleEnd = null)
        {
            if (state.CurrentPlayer == null) return;

            var battle = new TrpgBattle(state.CurrentPlayer, enemy);
            battle.OnBattleEnd = onBattleEnd;
            state.CurrentBattle = battle;

            // 이미 Combat 씬이면 씬 전환 불필요 (던전 연속 전투 등)
            if (state.CurrentScene != TrpgGameState.SceneType.Combat)
            {
                state.ChangeScene(TrpgGameState.SceneType.Combat);
            }

            battle.StartBattle(state);
        }




    }
}
