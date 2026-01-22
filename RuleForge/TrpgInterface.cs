using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class TrpgInterface
    {
        private static TrpgInterface _instance;
        public static TrpgInterface Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TrpgInterface();
                }
                return _instance;
            }
        }
        
        public TrpgInterface()
        {
            
        }
    }
}