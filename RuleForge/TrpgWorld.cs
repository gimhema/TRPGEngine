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
        public string Id { get; set; }
        public WorldBasicInfo basicInfo;
        public List<WorldUnit> ConnectedLocations { get; set; }

        public WorldUnit()
        {
            Id = Guid.NewGuid().ToString();
            basicInfo = new WorldBasicInfo();
            ConnectedLocations = new List<WorldUnit>();
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

        /// <summary>
        /// 한 방향 연결 추가 (this → destination)
        /// </summary>
        public void AddConnection(WorldUnit destination)
        {
            if (destination == null)
            {
                Console.WriteLine("연결할 목적지가 null입니다.");
                return;
            }

            if (destination == this)
            {
                Console.WriteLine("자기 자신과는 연결할 수 없습니다.");
                return;
            }

            if (ConnectedLocations.Contains(destination))
            {
                Console.WriteLine($"{destination.GetName()}은(는) 이미 연결되어 있습니다.");
                return;
            }

            ConnectedLocations.Add(destination);
        }

        protected abstract void Action();
    }

    public class Village : WorldUnit
    {
        public class Establishment
        {
            public string Name {get; set;} = "DEFAULT";
            public Establishment()
            {
            
            }

            public void Action()
            {
                
            }

        }

        public Dictionary<string, Establishment> establishments = new Dictionary<string, Establishment>();

        public Village()
        {
            
        }

        protected override void Action()
        {
            throw new NotImplementedException();
        }

    }


    public class Field : WorldUnit
    {
        public List<string> GatherableItems { get; set; }  // 채집 가능한 아이템 ID 목록
        public List<WorldUnit> ConnectedUnits {get; set;}
        public Field()
        {
            GatherableItems = new List<string>();
            ConnectedUnits = new List<WorldUnit>();
        }
        
        protected override void Action()
        {
            Console.WriteLine($"\n===== {basicInfo.Name} =====");
            Console.WriteLine(basicInfo.Description);
            
            bool exploring = true;
            while (exploring)
            {
                Console.WriteLine("\n[1] 채집하기");
                Console.WriteLine("[2] 주변 탐색");
                Console.WriteLine("[3] 다른 장소로 이동");
                Console.WriteLine("[4] 돌아가기");
                
                // TODO: 입력 처리 및 각 액션 구현
                // Gathering(), Explore(), Navigate() 등
            }
        }
        
        public void Gathering()
        {
            // 채집 로직
            Console.WriteLine("주변을 살펴보며 유용한 것들을 찾습니다...");
            // TODO: GatherableItems에서 랜덤 아이템 획득
        }
        
        public void Explore()
        {
            // 탐험 로직
            Console.WriteLine("주변을 탐험합니다...");
            // TODO: 랜덤 이벤트, 아이템 발견, 적 조우 등

            // ConnectedUnits의 정보들을 출력해준다.
        }

        public void AddWorldUnit(WorldUnit unit)
        {
            ConnectedUnits.Add(unit);
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
            // TODO: 실제 게임에서는 TrpgPlayer를 파라미터로 받거나
            // 게임 상태에서 현재 플레이어를 가져와야 함

            Console.WriteLine($"\n===== {basicInfo.Name} 진입 =====");
            Console.WriteLine(basicInfo.Description);

            // 던전 준비 (적 그룹 초기화)
            MakeDungeon();

            // 던전 루프
            while (EnemyGroupInstance.HasEnemies)
            {
                // Exploration()를 수행한다.
                Exploration();

                // 몬스터와 조우하게되면 EncountEnemy를 실행한다.
                if (EnemyGroupInstance.HasEnemies)
                {
                    EncountEnemy();

                    // TODO: 실제 전투 시스템이 구현되면 여기서 전투 결과를 확인하고
                    // 플레이어가 죽으면 Failed() 호출
                    // 적을 처치하면 계속 진행
                }
            }

            // 던전의 모든 몬스터들을 쓰러뜨리면 Clear
            // TODO: 실제로는 플레이어를 파라미터로 받아야 함
            // Clear(player);

            Console.WriteLine("\n던전의 모든 적을 물리쳤습니다!");
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
            // 던전 탐험 로직
            // TODO: 던전 내부 이동, 이벤트 발생 등의 로직 구현
            Console.WriteLine("던전을 탐험하고 있습니다...");
        }


        public void EncountEnemy()
        {
            var _enemy = EnemyGroupInstance.Encount();
            _enemy.EnemyAction();
        }

        public void GiveReward(TrpgPlayer player)
        {
            // 플레이어에게 ClearReward를 지급함
            if (ClearReward == null || ClearReward.Count == 0)
            {
                Console.WriteLine("던전 클리어 보상이 없습니다.");
                return;
            }

            Console.WriteLine($"\n던전 클리어 보상 {ClearReward.Count}개를 획득했습니다!");

            foreach (var rewardItem in ClearReward)
            {
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

        public void Failed(TrpgPlayer player)
        {
            // 던전 실패 조건은 플레이어의 죽음이다.
            Console.WriteLine("\n던전 공략 실패!");
            Console.WriteLine($"{player.Name}은(는) 쓰러졌습니다...");

            // 플레이어는 필드로 나가게 된다.
            Console.WriteLine("필드로 이동합니다.");

            // TODO: 실패 시 페널티 (아이템 손실, 경험치 감소 등) 구현 가능
        }

        public void Clear(TrpgPlayer player)
        {
            // 모든 몬스터들을 클리어하면 클리어 보상을 지급한다.
            Console.WriteLine("\n===== 던전 클리어! =====");
            Console.WriteLine($"{basicInfo.Name}의 모든 적을 처치했습니다!");

            IsClear = true;

            // 클리어 보상 지급
            GiveReward(player);

            // 플레이어는 필드로 나가게 된다.
            Console.WriteLine("\n필드로 이동합니다.");
        }

    }

}
