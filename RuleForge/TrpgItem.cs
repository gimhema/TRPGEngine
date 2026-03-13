using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    public class TrpgItem
    {
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }

        /// <summary>
        /// 아이템 기본 가격 (구매가). 판매가는 SellRatio를 곱한 값.
        /// </summary>
        public int Price { get; set; }

        public TrpgItem(string name, string description = "", int price = 0)
        {
            ItemName = name;
            ItemDescription = description;
            Price = price;
        }

        public void Use()
        {
            Console.WriteLine($"Item used: {ItemName}");
        }
    }

    public class Equipment : TrpgItem
    {
        enum EquipmentType
        {
            Default,
            Weapon,
            Armor,
            Accessory
        }

        private Dictionary<string, TrpgStatus> EquipmentStatuses = new Dictionary<string, TrpgStatus>();
        public Equipment(string name, string description = "", int price = 0) : base(name, description, price)
        {

        }           
        

        public TrpgStatus? GetStatusByName(string statusName)
        {
            if (EquipmentStatuses.ContainsKey(statusName))
            {
                return EquipmentStatuses[statusName];
            }
            return null;
        }
    }

    public class Consumable : TrpgItem
    {
        public int Quantity { get; set; }

        /// <summary>사용 시 HP 회복량 (0이면 효과 없음)</summary>
        public int HealHP { get; set; }

        /// <summary>사용 시 MP 회복량 (0이면 효과 없음)</summary>
        public int RestoreMP { get; set; }

        public Consumable(string name, string description = "", int price = 0) : base(name, description, price)
        {
        }

        // Quantity만 감소
        public new void Use()
        {
            if (Quantity > 0)
                Quantity--;
        }

        /// <summary>
        /// 아이템 효과를 target에 적용하고 결과 로그 목록을 반환한다.
        /// </summary>
        public List<string> ApplyEffect(TrpgActor target)
        {
            var logs = new List<string>();

            if (HealHP > 0)
            {
                int current = target.CommonAttributes.GetStatus("HP")?.StatusValue ?? 0;
                target.CommonAttributes.UpdateStatus("HP", current + HealHP);
                logs.Add($"{target.Name}의 HP가 {HealHP} 회복됐다!");
            }
            if (RestoreMP > 0)
            {
                int current = target.CommonAttributes.GetStatus("MP")?.StatusValue ?? 0;
                target.CommonAttributes.UpdateStatus("MP", current + RestoreMP);
                logs.Add($"{target.Name}의 MP가 {RestoreMP} 회복됐다!");
            }

            return logs;
        }
    }

    public class KeyItem : TrpgItem
    {
        public KeyItem(string name, string description = "", int price = 0) : base(name, description, price)
        {

        }

        // 부모 클래스의 Use 메서드를 오버라이드
        public new void Use()
        {
            Console.WriteLine($"Key item used: {ItemName}. It cannot be consumed.");
        }
    }

}