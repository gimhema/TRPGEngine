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

    public abstract class WorldUnit
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

        protected abstract void Action();
    }

    public class Village : WorldUnit
    {
        public Village()
        {
            
        }

        protected override void Action()
        {
            throw new NotImplementedException();
        }

    }

    public class Establishment : WorldUnit
    {
        
        public Establishment()
        {
            
        }
        
        protected override void Action()
        {
            throw new NotImplementedException();
        }

    }

    public class Field : WorldUnit
    {
        public Field()
        {
            
        }

        protected override void Action()
        {
            throw new NotImplementedException();
        }
    }

    public class Dungeon : WorldUnit
    {

        public bool IsClear {get; set;} = false;

        public List<TrpgEnemy> EnemyList;
        public TrpgEnemyGroup EnemyGroupInstance;

        public List<TrpgItem> ClearReward;

        public Dungeon()
        {
            EnemyGroupInstance = new TrpgEnemyGroup();
            ClearReward = new List<TrpgItem>();
        }        

        protected override void Action()
        {
            // 던전 루프에 진입한다.

            // Exploration()를 수행한다.

            // 몬스터와 조우하게되면 EncountEnemy를 실행한다.

            // 던전의 모든 몬스터들을 쓰러뜨리면 Clear, 진행도중 죽는다면 Failed를 실행한다.


            throw new NotImplementedException();
        }

        public void MakeDungeon()
        {
            foreach(var enemy in EnemyList)
            {
                EnemyGroupInstance.AddEnemy(enemy);
            }
        }

        public void Exploration()
        {
            
        }


        public void EncountEnemy()
        {
            var _enemy = EnemyGroupInstance.Encount();
            _enemy.EnemyAction();
        }

        public void GiveReward()
        {
            // 플레이어에게 ClearReward를 지급함
        }

        public void Failed()
        {
            // 던전 실패 조건은 플레이어의 죽음이다.

            // 플레이어는 필드로 나가게 된다.
        }

        public void Clear()
        {
            // 모든 몬스터들을 클리어하면 클리어 보상을 지급한다.

            // 플레이어는 필드로 나가게 된다.
        }

    }

}
