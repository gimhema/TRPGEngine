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
            BattleReward = new List<TrpgItem>();
        }

        /// <summary>
        /// 전투 스탯을 초기화한다. (HP, ATK, DEF, SPD)
        /// </summary>
        public void InitCombatStats(int hp, int atk, int def, int spd)
        {
            CommonAttributes.AddNewStatus("HP", hp);
            CommonAttributes.AddNewStatus("ATK", atk);
            CommonAttributes.AddNewStatus("DEF", def);
            CommonAttributes.AddNewStatus("SPD", spd);
        }

        public void EnemyAction()
        {
            
        }

        public void GiveReward(TrpgPlayer player)
        {
            // 플레이어에게 보상을 지급한다.
            if (BattleReward == null || BattleReward.Count == 0)
            {
                Console.WriteLine("보상이 없습니다.");
                return;
            }

            Random random = new Random();

            // 1 ~ BattleReward.Count 까지 보상의 가짓수를 정한다
            int rewardCount = random.Next(1, BattleReward.Count + 1);

            Console.WriteLine($"\n보상 {rewardCount}개를 획득했습니다!");

            // 이미 선택된 인덱스를 추적하여 중복 방지
            HashSet<int> selectedIndices = new HashSet<int>();

            // 보상의 가짓수만큼 랜덤하게 돌린다
            for (int i = 0; i < rewardCount; i++)
            {
                int randomIndex;

                // 중복되지 않은 인덱스를 찾을 때까지 반복
                do
                {
                    randomIndex = random.Next(0, BattleReward.Count);
                } while (selectedIndices.Contains(randomIndex));

                selectedIndices.Add(randomIndex);

                var rewardItem = BattleReward[randomIndex];

                // 아이템 타입에 따라 플레이어 인벤토리에 추가
                if (rewardItem is Consumable consumable)
                {
                    player.playerItemBag.AcquireConsumable(consumable);
                    Console.WriteLine($"  - {consumable.ItemName} 획득!");
                }
                else if (rewardItem is Equipment equipment)
                {
                    player.playerItemBag.AcquireEquipment(equipment);
                    Console.WriteLine($"  - {equipment.ItemName} 획득!");
                }
                else if (rewardItem is KeyItem keyItem)
                {
                    player.playerItemBag.KeyItems.Add(keyItem);
                    Console.WriteLine($"  - {keyItem.ItemName} 획득!");
                }
            }
        }

        public void Death(TrpgPlayer player)
        {
            // GiveReward()를 호출해서 플레이어에게 보상을 지급한다.
            Console.WriteLine($"\n{Name}을(를) 처치했습니다!");
            GiveReward(player);
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