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

        public Consumable(string name, string description = "", int price = 0) : base(name, description, price)
        {

        }

        // 부모 클래스의  Use 메서드를 오버라이드
        public new void Use()
        {
            if (Quantity > 0)
            {
                Quantity--;
                Console.WriteLine($"Consumable used: {ItemName}, Remaining quantity: {Quantity}");
            }
            else
            {
                Console.WriteLine($"No more {ItemName} left to use.");
            }
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