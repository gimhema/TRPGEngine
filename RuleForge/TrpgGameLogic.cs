using System;
using System.Collections.Generic;
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
        
    class RulebookParser
    {
        public List<Chapter> ParseRulebook(string filePath)
        {
            // 파일 읽기 및 파싱
            List<Chapter> chapters = new List<Chapter>();

            // Parse . . .

            return chapters;
        }
    }

    class TrpgGameLogic
    {
        private static TrpgGameLogic _instance;

        private List<Chapter> Chapters = new List<Chapter>();

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
        }
        

    }
}
