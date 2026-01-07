using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    public class TrpgActorStatus
    {
        public TrpgActorStatus() 
        {
            Hp = 0;
            Mp = 0;
            Attack = 0;
            Defense = 0;
            Dexterity = 0;
            Intelligence = 0;
            Experience = 0;
            Luck = 0;
        }

        public int Hp { get; set; }
        public int Mp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
          
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Experience { get; set; }

        public int Luck { get; set; }
    }

    // 장비와 아이템의 장착 및 사용효과는 모두... LLM이 Descripton을 읽고 판단한다.

    public class Equipment
    {
        public Equipment()
        {
            Name = string.Empty;
            Description = string.Empty;
        }
        public string Name { get; set; }
        public string Description { get; set; }

        public void Equip()
        {
            // Equip logic here
        }
    }

    public class Item
    {
        public Item()
        {
            Name = string.Empty;
            Description = string.Empty;
        }
        public string Name { get; set; }
        public string Description { get; set; }

        public void Use()
        {
            // Equip logic here
        }
    }

}
