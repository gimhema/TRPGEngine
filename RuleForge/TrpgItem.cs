using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class TrpgItem
    {
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }

        public TrpgItem(string name, string description = "")
        {
            ItemName = name;
            ItemDescription = description;
        }

        public void Use()
        {
            Console.WriteLine($"Item used: {ItemName}");
        }
    }

    class Equipment : TrpgItem
    {
        enum EquipmentType
        {
            Default,
            Weapon,
            Armor,
            Accessory
        }

        private EquipmentType TypeOfEquipment = EquipmentType.Default;

        private Dictionary<string, TrpgStatus> EquipmentStatuses = new Dictionary<string, TrpgStatus>();
        public Equipment(string name, string description = "") : base(name, description)
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

    class Consumable : TrpgItem
    {
        public int Quantity { get; set; }

        public Consumable(string name, string description = "") : base(name, description)
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

    class KeyItem : TrpgItem
    {
        public KeyItem(string name, string description = "") : base(name, description)
        {

        }

        // 부모 클래스의 Use 메서드를 오버라이드
        public new void Use()
        {
            Console.WriteLine($"Key item used: {ItemName}. It cannot be consumed.");
        }
    }

}