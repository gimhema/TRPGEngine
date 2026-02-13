using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    public class TrpgActor
    {
        public string Name { get; set; }

        public string Description { get; set; }
        public TrpgCommon CommonAttributes { get; set; }

        public TrpgActor()
        {
            Name = "None";
            Description = "None";
            CommonAttributes = new TrpgCommon();
        }

        public TrpgActor(string name, string description = "")
        {
            Name = name;
            Description = description;
            CommonAttributes = new TrpgCommon();
        }
    }
}
