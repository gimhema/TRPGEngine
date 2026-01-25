using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class PlayerClass
    {
        public string ClassName { get; set; }

        public PlayerClass(string className)
        {
            ClassName = className;
        }
    }

    class PlayerEquipments
    {
        Dictionary<string, Equipment> EquippedItems = new Dictionary<string, Equipment>();
        public PlayerEquipments()
        {
            
        }

        void EquipItem(Equipment item)
        {
            if (!EquippedItems.ContainsKey(item.ItemName))
            {
                EquippedItems.Add(item.ItemName, item);
            }
        }

        void UnequipItem(string itemName)
        {
            if (EquippedItems.ContainsKey(itemName))
            {
                EquippedItems.Remove(itemName);
            }
        }
    }

    class PlayerItemBag
    {
        public List<Consumable> ConsumableItems { get; set; }
        public List<Equipment> EquipmentItems { get; set; }

        public PlayerItemBag()
        {
            ConsumableItems = new List<Consumable>();
            EquipmentItems = new List<Equipment>();
        }
    }

    class TrpgPlayer : TrpgActor
    {
        // 클래스의 타입은 룰북에서 정의된것을 사용해야하기때문에 enum을 활용하지않는다.



        public PlayerClass playerClass { get; set; }
        public PlayerItemBag playerItemBag { get; set; }
        public PlayerEquipments playerEquipments { get; set; }
        public TrpgPlayer(string name, string description = "", string className = "") : base(name, description)
        {
            playerClass = new PlayerClass(className);
        }
    }

    
}
