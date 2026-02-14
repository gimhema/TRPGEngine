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
        
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestType Type { get; set; }

        public bool IsCompleted { get; set; } = false;

        public Quest(string title, QuestType type)
        {
            Title = title;
            Type = type;
            Description = "";
        }

        public string GetNarrative()
        {
            return $"Title : {Title}\n{Description}";
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
        /// 게임 시작 (GameState 초기화)
        /// </summary>
        public void InitializeGame(TrpgGameState state)
        {
            state.NarrativeText = "게임에 오신 것을 환영합니다!";
            state.CurrentScene = TrpgGameState.SceneType.PlayerSetup;
        }
        
        public void DoAction(string actionName)
        {
            GameRule.DoAction(actionName);
        }

        /// <summary>
        /// 입력 처리 (GameController에서 호출)
        /// </summary>
        public void ProcessInput(string input, TrpgGameState state)
        {
            // 현재 씬에 따라 다르게 처리
            switch (state.CurrentScene)
            {
                case TrpgGameState.SceneType.PlayerSetup:
                    // 플레이어 설정 중에는 선택지를 통해 처리
                    break;
                case TrpgGameState.SceneType.Exploration:
                    // 탐험 중 입력 처리
                    break;
                case TrpgGameState.SceneType.Combat:
                    // 전투 중 입력 처리
                    break;
                default:
                    state.NarrativeText = $"처리되지 않은 입력: {input}";
                    break;
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        public TrpgPlayer CreatePlayer(string name, int age = 18, string gender = "Not Specified",
            string personality = "Neutral", string job = "Adventurer", string backgroundStory = "A mysterious past.")
        {
            var newPlayer = new TrpgPlayer(name);
            newPlayer.playerProfile.age = age;
            newPlayer.playerProfile.gender = gender;
            newPlayer.playerProfile.personality = personality;
            newPlayer.playerProfile.job = job;
            newPlayer.playerProfile.backgroundStory = backgroundStory;
            newPlayer.playerProfile.PlayerLevel = 1;

            // 기본 스탯 설정
            newPlayer.CommonAttributes.AddNewStatus("HP", 100);
            newPlayer.CommonAttributes.AddNewStatus("MP", 50);
            newPlayer.CommonAttributes.AddNewStatus("ATK", 15);
            newPlayer.CommonAttributes.AddNewStatus("DEF", 10);
            newPlayer.CommonAttributes.AddNewStatus("SPD", 10);

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
