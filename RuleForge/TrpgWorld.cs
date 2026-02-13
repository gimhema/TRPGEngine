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

        public World? SelectWorld(string selected)
        {
            if (Worlds.TryGetValue(selected, out var _world))
                return _world;
            return null;
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

        /// <summary>
        /// 해당 월드 유닛에 진입했을 때의 동작을 설정한다.
        /// TrpgGameState에 내러티브와 선택지를 설정하여 게임 루프에서 처리하도록 한다.
        /// </summary>
        public abstract void Action(TrpgGameState state);
    }

    public class Village : WorldUnit
    {
        public class Establishment
        {
            public string Name {get; set;} = "DEFAULT";
            public Dictionary<string, TrpgNPC> npcs;
            public Establishment()
            {
                npcs = new Dictionary<string, TrpgNPC>();
            }

            /// <summary>
            /// 시설에 진입했을 때의 동작을 설정한다.
            /// NPC 목록을 선택지로 보여주고, 돌아가기 옵션을 제공한다.
            /// </summary>
            public void Action(TrpgGameState state, Village parentVillage)
            {
                state.NarrativeText = $"[{Name}]에 들어왔습니다.";
                state.ClearChoices();

                int index = 1;
                foreach (var kvp in npcs)
                {
                    var npc = kvp.Value;
                    string choiceId = index.ToString();
                    state.AddChoice(new TrpgChoice(choiceId, $"{index}. {npc.Name}과(와) 대화하기")
                    {
                        OnSelect = (s) =>
                        {
                            // TODO: NPC 대화 시스템 구현 후 연결
                            s.NarrativeText = $"{npc.Name}과(와) 대화를 시작합니다.";
                        }
                    });
                    index++;
                }

                // 돌아가기 - 마을 시설 목록으로 복귀
                string backId = index.ToString();
                state.AddChoice(new TrpgChoice(backId, $"{index}. 돌아가기")
                {
                    OnSelect = (s) =>
                    {
                        parentVillage.Action(s);
                    }
                });
            }

            public TrpgNPC? SelectNPC(string npcName)
            {
                if (npcs.TryGetValue(npcName, out var npc))
                return npc;
            return null;
            }

        }

        public Dictionary<string, Establishment> establishments = new Dictionary<string, Establishment>();

        public Village()
        {
            
        }

        public Establishment? SelectEstablishment(string selected)
        {
            if (establishments.TryGetValue(selected, out var establishment))
                return establishment;
            return null;
        }

        /// <summary>
        /// 마을에 진입했을 때의 동작.
        /// 마을 내 시설 목록을 선택지로 보여주고, 시설 선택 또는 돌아가기를 처리한다.
        /// </summary>
        public override void Action(TrpgGameState state)
        {
            state.NarrativeText = $"===== {GetName()} =====\n{GetDescription()}";
            state.ClearChoices();

            int index = 1;
            foreach (var kvp in establishments)
            {
                var establishment = kvp.Value;
                string choiceId = index.ToString();
                state.AddChoice(new TrpgChoice(choiceId, $"{index}. {establishment.Name}")
                {
                    OnSelect = (s) =>
                    {
                        establishment.Action(s, this);
                    }
                });
                index++;
            }

            // 돌아가기 - 이전 씬으로 복귀
            string backId = index.ToString();
            state.AddChoice(new TrpgChoice(backId, $"{index}. 마을을 떠나기")
            {
                OnSelect = (s) =>
                {
                    s.ReturnToPreviousScene();
                }
            });
        }

    }


    public class Field : WorldUnit
    {
        public List<string> GatherableItems { get; set; }  // 채집 가능한 아이템 ID 목록
        public Field()
        {
            GatherableItems = new List<string>();
        }
        
        public override void Action(TrpgGameState state)
        {
            state.NarrativeText = $"===== {basicInfo.Name} =====\n{basicInfo.Description}";
            state.ClearChoices();

            state.AddChoice(new TrpgChoice("1", "1. 채집하기")
            {
                OnSelect = (s) =>
                {
                    Gathering(s);
                }
            });
            state.AddChoice(new TrpgChoice("2", "2. 주변 탐색")
            {
                OnSelect = (s) =>
                {
                    Explore(s);
                }
            });
            state.AddChoice(new TrpgChoice("3", "3. 돌아가기")
            {
                OnSelect = (s) => { s.ReturnToPreviousScene(); }
            });
        }

        public void Gathering(TrpgGameState state)
        {
            // TODO: GatherableItems에서 랜덤 아이템 획득 후 플레이어 인벤토리에 추가
            // TODO: 플레이어 액션 시스템 구현 후 연결
            state.NarrativeText = "주변을 살펴보며 유용한 것들을 찾습니다...";
            Action(state);
        }

        /// <summary>
        /// 현재 필드와 연결된 WorldUnit 목록을 선택지로 보여준다.
        /// 선택 시 해당 WorldUnit의 Action()으로 진입한다.
        /// </summary>
        public void Explore(TrpgGameState state)
        {
            state.NarrativeText = $"===== {basicInfo.Name} - 주변 탐색 =====\n주변을 둘러봅니다...";
            state.ClearChoices();

            if (ConnectedLocations.Count == 0)
            {
                state.NarrativeText += "\n\n주변에 이동할 수 있는 장소가 없습니다.";
            }

            int index = 1;
            foreach (var unit in ConnectedLocations)
            {
                var targetUnit = unit;
                string choiceId = index.ToString();
                state.AddChoice(new TrpgChoice(choiceId, $"{index}. {targetUnit.GetName()}", targetUnit.GetDescription())
                {
                    OnSelect = (s) =>
                    {
                        targetUnit.Action(s);
                    }
                });
                index++;
            }

            // 돌아가기 - 현재 필드 메뉴로 복귀
            string backId = index.ToString();
            state.AddChoice(new TrpgChoice(backId, $"{index}. 돌아가기")
            {
                OnSelect = (s) =>
                {
                    Action(s);
                }
            });
        }
        

        public WorldUnit? SelectExplore(int selectIdx)
        {
            if (selectIdx < 0 || selectIdx >= ConnectedLocations.Count)
                return null;
            return ConnectedLocations[selectIdx];
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

        public override void Action(TrpgGameState state)
        {
            state.NarrativeText = $"===== {basicInfo.Name} 진입 =====\n{basicInfo.Description}";
            state.ClearChoices();

            // 던전 준비 (적 그룹 초기화)
            MakeDungeon();

            state.AddChoice(new TrpgChoice("1", "1. 던전 탐험 시작"));
            state.AddChoice(new TrpgChoice("2", "2. 돌아가기")
            {
                OnSelect = (s) => { s.ReturnToPreviousScene(); }
            });

            // TODO: 던전 루프를 게임 상태 기반으로 재구성 필요
            // 전투 시스템 구현 후 연결
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
