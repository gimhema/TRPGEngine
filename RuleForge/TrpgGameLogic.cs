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
        private static TrpgGameLogic _instance;
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

        }

        public void StartGame()
        {
            Console.WriteLine("TRPG Game Started!");
        }
        

    }
}
