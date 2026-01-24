using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RuleForge
{
    class TrpgGameSituation
    {
        public string SituationName { get; set; }
        public string SituationDescription { get; set; }

        public List<TrpgGameAction> PossibleActions { get; set; }

        public TrpgGameSituation()
        {
            SituationName = string.Empty;
            SituationDescription = string.Empty;
            PossibleActions = new List<TrpgGameAction>();
        }
    }
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

    }
}
