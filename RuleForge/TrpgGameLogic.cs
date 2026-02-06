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

        public void Start()
        {
            Console.WriteLine($"Chapter started: {Title}");
            foreach (var quest in Quests)
            {
                Console.WriteLine($"Starting quest: {quest.Title} ({quest.Type})");
            }
        }

        public void SelectQuest(int index)
        {
            if (index >= 0 && index < Quests.Count)
            {
                currentQuestIndex = index;
                Console.WriteLine($"Quest selected: {Quests[index].Title}");
            }
            else
            {
                Console.WriteLine("Invalid quest index.");
            }
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

        public void Narrate()
        {
            Console.WriteLine($"Title : {Title}");
            Console.WriteLine($"{Description}");
        }

    }

    // Activity
    class Activity
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

        public void SelectActivity()
        {
            Console.WriteLine($"Activity selected: {Type}");
            Do();
        }

        public void Do()
        {
            switch(Type)
            {
                case ActivityType.Combat:
                    Console.WriteLine("Engaging in combat...");
                    break;
                case ActivityType.Exploration:
                    Console.WriteLine("Exploring the area...");
                    break;
                case ActivityType.Social:
                    Console.WriteLine("Interacting with NPCs...");
                    break;
            }
        }

    }
        

    class TrpgGameLogic
    {
        private static TrpgGameLogic? _instance;
        private TrpgRule GameRule = new TrpgRule();
        private List<Chapter> Chapters = new List<Chapter>();

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

        public void StartGame()
        {
            IntroduceGame();
            
            PlayerSetting();

            GameLoop();
        }

        public void GameLoop()
        {
            while (true)
            {
                // Main game loop logic
                Console.WriteLine("Game is running...");

                if(IsGameExit)
                {
                    // 인터페에스에서 종료 처리를 했을경우
                    // 바로 종료시킴
                    Console.WriteLine("Exiting game...");
                    break;
                }

                if(IsGameCleard)
                {
                    // 게임 클리어시 처리
                    // 종료시키진않고 인터페이스에서 홈화면으로 되돌림
                    Console.WriteLine("Congratulations! You have cleared the game!");
                    break;
                }

            // if (Chapters.Count > 0)
            // {
            //     Chapters.First().Start(); 
            // }
            // else
            // {
            //     Console.WriteLine("No chapters available.");
            // }

                // For demonstration, we'll just break the loop
                break;
            }
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
            // 추후 구현 예정
            Console.WriteLine($"처리되지 않은 입력: {input}");
        }

        public void IntroduceGame()
        {
            // Print Game introduction
        }

        public void PlayerSetting()
        {

            // 우선 싱글 플레이어 모드만 고려
            Console.WriteLine("Enter your player name:");
            string playerName = Console.ReadLine() ?? "Player1";

            TrpgPlayer newPlayer = new TrpgPlayer(playerName);

            Console.WriteLine("Next.. What's your age?");
            string ageInput = Console.ReadLine() ?? "18";
            if (int.TryParse(ageInput, out int age))
            {
                newPlayer.playerProfile.age = age;
            }
            else
            {
                newPlayer.playerProfile.age = 18; // Default age
            }

            Console.WriteLine("Tell mey about yout gender:");
            string genderInput = Console.ReadLine() ?? "Not Specified";
            newPlayer.playerProfile.gender = genderInput;

            Console.WriteLine("And describe your personality:");
            string personalityInput = Console.ReadLine() ?? "Neutral";  
            newPlayer.playerProfile.personality = personalityInput;

            Console.WriteLine("What's your job?");
            string jobInput = Console.ReadLine() ?? "Adventurer";
            newPlayer.playerProfile.job = jobInput;

            Console.WriteLine("Finally, share a bit of your background story:");
            string backgroundInput = Console.ReadLine() ?? "A mysterious past.";
            newPlayer.playerProfile.backgroundStory = backgroundInput;

            Console.WriteLine("Player profile created successfully!");

            Players.Add(playerName, newPlayer);



            // 나중에 멀티 모드도 고려해야함

        }




    }
}
