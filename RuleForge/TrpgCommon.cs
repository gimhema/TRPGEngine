using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class TrpgStatus
    {
        public string StatusName { get; set; }
        public int StatusValue { get; set; }

        public TrpgStatus(string name, int value)
        {
            StatusName = name;
            StatusValue = value;
        }
    }
    class TrpgCommon
    {
        public Dictionary<string, TrpgStatus> Statuses { get; set; }

        public TrpgCommon()
        {
            Statuses = new Dictionary<string, TrpgStatus>();
        }

        public void AddNewStatus(string name, int value)
        {
            if (!Statuses.ContainsKey(name))
            {
                Statuses[name] = new TrpgStatus(name, value);
            }
        }

        public void UpdateStatus(string name, int value)
        {
            if (Statuses.ContainsKey(name))
            {
                Statuses[name].StatusValue = value;
            }
        }

        public TrpgStatus? GetStatus(string name)
        {
            if (Statuses.ContainsKey(name))
            {
                return Statuses[name];
            }
            return null;
        }

    }


}
