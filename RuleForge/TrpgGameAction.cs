using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class TrpgGameAction
    {
        public string ActionName { get; set; }
        public string ActionDescription { get; set; }

        public TrpgGameAction(string name, string description = "")
        {
            ActionName = name;
            ActionDescription = description;
        }

        public int DoDiceRoll(int numberOfDice = 1, int sidesPerDie = 6)
        {
            Random rand = new Random();
            int total = 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                total += rand.Next(1, sidesPerDie + 1);
            }
            return total;
        }

        public int DoAction(TrpgActor actor)
        {
            // 액션 수행 로직 구현
            // 예: 특정 상태를 변경하거나, 주사위를 굴리는 등의 작업
            int _diceResult = DoDiceRoll();
            Console.WriteLine($"{actor.Name} performs action: {ActionName}");
            return 0; // 결과값 반환 (예: 성공 여부, 피해량 등)
        }
    }
}
