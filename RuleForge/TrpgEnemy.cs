using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{

    public class TrpgEnemy : TrpgActor
    {
        
        public TrpgEnemy(string name, string description = "", string className = "") : base(name, description)
        {

        }

    }

    public class TrpgEnemyGroup
    {
        public List<TrpgEnemy> Enemys;
        public TrpgEnemyGroup()
        {
            Enemys = new List<TrpgEnemy>();
        }
    }

}