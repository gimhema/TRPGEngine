using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RuleForge
{

    class TrpgGameAction
    {
        public string ActionName { get; set; }
        public string ActionDescription { get; set; }

        public TrpgActor? ActionTargetActor { get; set; }

        public TrpgStatus? ActionCost { get; set; }

        public TrpgGameAction()
        {
            ActionName = string.Empty;
            ActionDescription = string.Empty;
        }

        public void ExecuteAction()
        {
            Console.WriteLine($"Action executed: {ActionName}");
        }

    }
}
