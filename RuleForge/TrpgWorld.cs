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
        /// 해당 월드 유닛에 진입했을 때의 동작.
        /// CurrentLocation을 갱신한 뒤, 하위 클래스의 OnAction()을 호출한다.
        /// </summary>
        public void Action(TrpgGameState state)
        {
            state.CurrentLocation = this;
            OnAction(state);
        }

        /// <summary>
        /// 하위 클래스에서 구현할 실제 진입 동작.
        /// 내러티브와 선택지를 TrpgGameState에 설정한다.
        /// </summary>
        protected abstract void OnAction(TrpgGameState state);
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
                            if (npc.NpcType == "Trader" && npc.TradeItems.Count > 0)
                            {
                                var shop = new TrpgShop(npc.Name);
                                shop.Merchandise.AddRange(npc.TradeItems);
                                shop.Enter(s);
                            }
                            else
                            {
                                // TODO: NPC 대화 시스템 구현 후 연결
                                s.NarrativeText = $"{npc.Name}과(와) 대화를 시작합니다.";
                            }
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
        protected override void OnAction(TrpgGameState state)
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

            // 휴식하기
            state.AddChoice(new TrpgChoice(index.ToString(), $"{index}. 휴식하기")
            {
                OnSelect = (s) =>
                {
                    if (s.CurrentPlayer != null)
                    {
                        var player = s.CurrentPlayer;
                        var initStats = TrpgGameConfig.PlayerDefault.InitialStats;

                        int restoreHp = initStats.TryGetValue("HP", out int maxHp) ? maxHp : 100;
                        int restoreMp = initStats.TryGetValue("MP", out int maxMp) ? maxMp : 50;

                        player.CommonAttributes.UpdateStatus("HP", restoreHp);
                        player.CommonAttributes.UpdateStatus("MP", restoreMp);
                        s.NarrativeText = $"===== {GetName()} =====\n{GetName()}에서 편히 쉬었습니다.\nHP와 MP가 완전히 회복되었습니다.";
                    }
                    else
                    {
                        s.NarrativeText = "휴식을 취합니다...";
                    }
                    s.ClearChoices();
                    s.AddChoice(new TrpgChoice("1", "1. 계속")
                    {
                        OnSelect = (ns) => Action(ns)
                    });
                }
            });
            index++;

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
        
        protected override void OnAction(TrpgGameState state)
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
            if (state.CurrentPlayer == null || GatherableItems.Count == 0)
            {
                state.NarrativeText = "이 지역에서 채집할 수 있는 것이 없습니다.";
                Action(state);
                return;
            }

            var random = new Random();
            string itemIdStr = GatherableItems[random.Next(GatherableItems.Count)];

            if (int.TryParse(itemIdStr, out int itemId))
            {
                var templateItem = TrpgItemRegistry.Get(itemId);
                if (templateItem is Consumable consumable)
                {
                    var newItem = new Consumable(consumable.ItemName, consumable.ItemDescription, consumable.Price)
                    {
                        HealHP = consumable.HealHP,
                        RestoreMP = consumable.RestoreMP,
                        Quantity = 1
                    };
                    state.CurrentPlayer.playerItemBag.AcquireConsumable(newItem);
                    state.NarrativeText = $"채집: {newItem.ItemName}을(를) 발견했습니다!";
                }
                else if (templateItem is Equipment equipment)
                {
                    var newItem = new Equipment(equipment.ItemName, equipment.ItemDescription, equipment.Price);
                    state.CurrentPlayer.playerItemBag.AcquireEquipment(newItem);
                    state.NarrativeText = $"채집: {newItem.ItemName}을(를) 발견했습니다!";
                }
                else
                {
                    state.NarrativeText = "주변을 살펴봤지만 아무것도 찾지 못했습니다.";
                }
            }
            else
            {
                state.NarrativeText = "주변을 살펴봤지만 아무것도 찾지 못했습니다.";
            }

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
        public bool IsClear { get; set; } = false;

        public List<TrpgEnemy> EnemyList;
        public TrpgEnemyGroup EnemyGroupInstance;

        public List<TrpgItem> ClearReward;

        public Dungeon()
        {
            EnemyList = new List<TrpgEnemy>();
            EnemyGroupInstance = new TrpgEnemyGroup();
            ClearReward = new List<TrpgItem>();
        }

        protected override void OnAction(TrpgGameState state)
        {
            state.NarrativeText = $"===== {basicInfo.Name} 진입 =====\n{basicInfo.Description}";
            state.ClearChoices();

            if (IsClear)
            {
                state.NarrativeText += "\n\n이미 클리어한 던전입니다.";
                state.AddChoice(new TrpgChoice("1", "1. 돌아가기")
                {
                    OnSelect = (s) => { s.ReturnToPreviousScene(); }
                });
                return;
            }

            // 던전 준비 (적 그룹 초기화)
            MakeDungeon();

            int enemyCount = EnemyGroupInstance.RemainingEnemies;
            state.NarrativeText += $"\n\n적 {enemyCount}마리가 기다리고 있다...";

            state.AddChoice(new TrpgChoice("1", "1. 던전 탐험 시작")
            {
                OnSelect = (s) => { StartDungeonExploration(s); }
            });
            state.AddChoice(new TrpgChoice("2", "2. 돌아가기")
            {
                OnSelect = (s) => { s.ReturnToPreviousScene(); }
            });
        }

        /// <summary>
        /// 적 그룹을 초기화한다. 기존 큐를 비우고 EnemyList에서 다시 등록한다.
        /// </summary>
        public void MakeDungeon()
        {
            EnemyGroupInstance.Clear();
            foreach (var enemy in EnemyList)
            {
                EnemyGroupInstance.AddEnemy(enemy);
            }
        }

        /// <summary>
        /// 던전 탐험을 시작한다. 다음 적과 전투를 개시한다.
        /// </summary>
        private void StartDungeonExploration(TrpgGameState state)
        {
            if (!EnemyGroupInstance.HasEnemies)
            {
                Clear(state);
                return;
            }

            var enemy = EnemyGroupInstance.Encount();
            if (enemy == null)
            {
                Clear(state);
                return;
            }

            TrpgGameLogic.Instance.StartBattle(enemy, state, ContinueDungeonExploration);
        }

        /// <summary>
        /// 전투 종료 후 콜백. 승리 시 다음 적과 전투, 모두 처치 시 클리어, 패배 시 실패 처리.
        /// </summary>
        private void ContinueDungeonExploration(TrpgGameState state, bool isVictory)
        {
            if (!isVictory)
            {
                Failed(state);
                return;
            }

            if (EnemyGroupInstance.HasEnemies)
            {
                // 다음 적과 전투 (이미 Combat 씬이므로 씬 전환 불필요)
                var nextEnemy = EnemyGroupInstance.Encount();
                if (nextEnemy != null)
                {
                    int remaining = EnemyGroupInstance.RemainingEnemies;
                    state.NarrativeText = $"다음 적이 나타난다! (남은 적: {remaining})";
                    state.ClearChoices();
                    state.AddChoice(new TrpgChoice("1", "1. 전투 계속")
                    {
                        OnSelect = (s) =>
                        {
                            TrpgGameLogic.Instance.StartBattle(nextEnemy, s, ContinueDungeonExploration);
                        }
                    });
                    return;
                }
            }

            Clear(state);
        }

        /// <summary>
        /// 던전 클리어 처리. 보상을 지급하고 필드로 복귀할 수 있게 한다.
        /// </summary>
        public void Clear(TrpgGameState state)
        {
            IsClear = true;

            var sb = new StringBuilder();
            sb.AppendLine("===== 던전 클리어! =====");
            sb.AppendLine($"{basicInfo.Name}의 모든 적을 처치했습니다!");

            // 클리어 보상 지급
            if (ClearReward.Count > 0 && state.CurrentPlayer != null)
            {
                sb.AppendLine($"\n던전 클리어 보상 {ClearReward.Count}개를 획득!");
                foreach (var rewardItem in ClearReward)
                {
                    if (rewardItem is Consumable consumable)
                    {
                        state.CurrentPlayer.playerItemBag.AcquireConsumable(consumable);
                        sb.AppendLine($"  - {consumable.ItemName} 획득!");
                    }
                    else if (rewardItem is Equipment equipment)
                    {
                        state.CurrentPlayer.playerItemBag.AcquireEquipment(equipment);
                        sb.AppendLine($"  - {equipment.ItemName} 획득!");
                    }
                    else if (rewardItem is KeyItem keyItem)
                    {
                        state.CurrentPlayer.playerItemBag.KeyItems.Add(keyItem);
                        sb.AppendLine($"  - {keyItem.ItemName} 획득!");
                    }
                }
            }

            state.NarrativeText = sb.ToString();
            state.ClearChoices();
            state.AddChoice(new TrpgChoice("1", "1. 필드로 돌아가기")
            {
                OnSelect = (s) => { s.ReturnToPreviousScene(); }
            });
        }

        /// <summary>
        /// 던전 실패 처리. HP를 1로 회복하고 필드로 복귀할 수 있게 한다.
        /// </summary>
        public void Failed(TrpgGameState state)
        {
            var playerName = state.CurrentPlayer?.Name ?? "플레이어";
            state.NarrativeText = $"===== 던전 공략 실패! =====\n{playerName}은(는) 쓰러졌습니다...\n\n필드로 이동합니다.";
            state.ClearChoices();
            state.AddChoice(new TrpgChoice("1", "1. 필드로 돌아가기")
            {
                OnSelect = (s) =>
                {
                    // HP를 1로 회복 (게임 오버 방지)
                    if (s.CurrentPlayer != null)
                    {
                        s.CurrentPlayer.CommonAttributes.UpdateStatus("HP", 1);
                    }
                    s.ReturnToPreviousScene();
                }
            });
        }
    }

}
