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
        private Dictionary<string, TrpgStatus> EquipmentStatuses = new Dictionary<string, TrpgStatus>();
        public Equipment(string name, string description = "") : base(name, description)
        {
            
        }

        public TrpgStatus? GetStatusByName(string statusName)
        {
            if (EquipmentStatuses.ContainsKey(statusName))
            {
                return EquipmentStatuses[statusName];
            }
            return null;
        }
    }

    class Consumable : TrpgItem
    {
        public Consumable(string name, string description = "") : base(name, description)
        {
            
        }

        public void UseItem(TrpgActor targetActor)
        {
            Console.WriteLine($"{targetActor.Name} used {ItemName}.");
        }
    }

}