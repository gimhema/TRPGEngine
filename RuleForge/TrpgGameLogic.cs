using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    // Chapter
    class Chapter
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
    class Quest
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
            Console.WriteLine("TRPG Game Started!");

            if (Chapters.Count > 0)
            {
                Chapters.First().Start(); 
            }
            else
            {
                Console.WriteLine("No chapters available.");
            }
        }
        
        public void DoAction(string actionName)
        {
            GameRule.DoAction(actionName);
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
