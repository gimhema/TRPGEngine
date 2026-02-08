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

        public void EnemyAction()
        {
            
        }

    }

    public class TrpgEnemyGroup
    {
        public Queue<TrpgEnemy> EncounterQueue;
        public int RemainingEnemies => EncounterQueue.Count;
        public bool HasEnemies => EncounterQueue.Count > 0;
        public TrpgEnemyGroup()
        {
            EncounterQueue = new Queue<TrpgEnemy>();
        }

        public void AddEnemy(TrpgEnemy enemy)
        {
            EncounterQueue.Enqueue(enemy);
        }

        public void Clear()
        {
            EncounterQueue.Clear();
        }

        public TrpgEnemy? Encount()
        {
            if (EncounterQueue.Count > 0)
                return EncounterQueue.Dequeue();
            return null;
        }

    }

}