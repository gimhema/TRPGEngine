using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{

    public class TrpgEnemy : TrpgActor
    {
        
        public List<TrpgItem> BattleReward;

        public TrpgEnemy(string name, string description = "", string className = "") : base(name, description)
        {

        }

        public void EnemyAction()
        {
            
        }

        public void GiveReward()
        {
            // 플레이어에게 보상을 지급한다.

            // 이때 랜덤으로 지급을 하는데
            // 1 ~ BattleReward.size() 까지 보상의 가짓수를 정할수있다.
            // 보상의 가짓수만큼 랜덤하게 돌린다.
            // 이때 루프내에서 인덱스도 랜덤하게 돌려서 지급한다.

        }

        public void Death()
        {
            // GiveReward()를 호출해서 플레이어에게 보상을 지급한다.
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