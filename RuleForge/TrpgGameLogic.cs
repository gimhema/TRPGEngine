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
        // Main Quest
        // Sub Quest
    class Quest
    {
        
    }

    // Activity
        // Combat Acitivity
        // Exploration Activity
        // Social Activity 
    class Activity
    {
        
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
