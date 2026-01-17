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
        public List<Quest> Scenes { get; set; }

        public Chapter(string title)
        {
            Title = title;
            Scenes = new List<Quest>();
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
        public QuestType Type { get; set; }

        public Quest(string title, QuestType type)
        {
            Title = title;
            Type = type;
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
