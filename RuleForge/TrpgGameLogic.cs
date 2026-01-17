using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
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
