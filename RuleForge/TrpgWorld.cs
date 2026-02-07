using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    public class WorldManager
    {
        public Dictionary<string, World> Worlds;

        public WorldManager()
        {
            Worlds =  new Dictionary<string, World>();
        }

        public void LoadWorldInfoByRuleBook()
        {
            // Parse World Info from RuleBook . . .
        }
    }

    public class WorldBasicInfo
    {
        public string Name {get; set;}
        public string Description {get; set;}

        public WorldBasicInfo()
        {
            Name = "Default";
            Description = "Empty";
        }
    }

    public class World
    {
        public WorldBasicInfo basicInfo;
        public Dictionary<string, WorldUnit> units;

        public World()
        {
            basicInfo = new WorldBasicInfo();
        }

        public void SetWorldInfo(string _name, string _desc)
        {
            basicInfo.Name = _name;
            basicInfo.Description = _desc;
        }

        public string GetName()
        {
            return basicInfo.Name;
        }

        public string GetDescription()
        {
            return basicInfo.Description;
        }
        
    }

    public class WorldUnit
    {
        public WorldBasicInfo basicInfo;

        public WorldUnit()
        {
            basicInfo = new WorldBasicInfo();
        }

        public void SetWorldInfo(string _name, string _desc)
        {
            basicInfo.Name = _name;
            basicInfo.Description = _desc;
        }

        public string GetName()
        {
            return basicInfo.Name;
        }

        public string GetDescription()
        {
            return basicInfo.Description;
        }

        public void Action()
        {
            // 여기에 상속받는 자식 클래스 별로 행동할수있게끔
        } 
    }

    public class Village : WorldUnit
    {
        public Village()
        {
            
        }

    }

    public class Establishment : WorldUnit
    {
        
        public Establishment()
        {
            
        }
        
    }

    public class Field : WorldUnit
    {
        public Field()
        {
            
        }
    }

    public class Dungeon : WorldUnit
    {
        public Dungeon()
        {
            
        }        

    }




}
