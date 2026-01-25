using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class TrpgItem
    {
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }

        public TrpgItem(string name, string description = "")
        {
            ItemName = name;
            ItemDescription = description;
        }
    }

    class Equipment : TrpgItem
    {
        public Equipment(string name, string description = "") : base(name, description)
        {
            
        }
    }

    class Consumable : TrpgItem
    {
        public Consumable(string name, string description = "") : base(name, description)
        {
            
        }
    }

}