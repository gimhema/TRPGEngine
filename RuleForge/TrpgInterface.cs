using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    /// <summary>
    /// [DEPRECATED] 이 클래스는 더 이상 사용되지 않습니다.
    /// 대신 다음을 사용하세요:
    /// - TrpgGameController: 게임 루프 관리
    /// - TrpgInputHandler: 입력 처리
    /// - TrpgRenderer: 화면 출력
    /// - TrpgGameState: 게임 상태 관리
    /// </summary>
    [Obsolete("Use TrpgGameController, TrpgInputHandler, and TrpgRenderer instead")]
    class TrpgInterface
    {
        private static TrpgInterface? _instance;
        private Activity? _currentActivity;
        private TrpgPlayer? _currentPlayer;

        public Activity? CurrentActivity
        {
            get { return _currentActivity; }
            set { _currentActivity = value; }
        }

        public TrpgPlayer? CurrentPlayer
        {
            get { return _currentPlayer; }
            set { _currentPlayer = value; }
        }

        public static TrpgInterface Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TrpgInterface();
                }
                return _instance;
            }
        }
        
        public TrpgInterface()
        {
            
        }

        public async Task InputSpin()
        {
            while (true)
            {
                InputAction();
                await Task.Delay(100);
            }
        }

        public void InputAction()
        {
            if (_currentActivity == null)
            {
                HandleMainMenu();
                return;
            }

            switch (_currentActivity.Type)
            {
                case Activity.ActivityType.Combat:
                    HandleBattleInput();
                    break;
                case Activity.ActivityType.Exploration:
                    HandleExplorationInput();
                    break;
                case Activity.ActivityType.Social:
                    HandleSocialInput();
                    break;
                default:
                    Console.WriteLine("Unknown activity type.");
                    break;
            }
        }

        private void HandleMainMenu()
        {
            Console.WriteLine("\n===== MAIN MENU =====");
            Console.WriteLine("1. Start Game");
            Console.WriteLine("2. Load Game");
            Console.WriteLine("3. Settings");
            Console.WriteLine("4. Exit");
            Console.Write("Select an option: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Starting new game...");
                    _currentActivity = new Activity(Activity.ActivityType.Exploration);
                    // DEPRECATED: Use TrpgGameController instead
                    // TrpgGameLogic.Instance.StartGame();
                    break;
                case "2":
                    Console.WriteLine("Loading saved game...");
                    _currentActivity = new Activity(Activity.ActivityType.Exploration);
                    break;
                case "3":
                    Console.WriteLine("Opening settings...");
                    break;
                case "4":
                    Console.WriteLine("Exiting game...");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 전투 중 입력 처리
        /// </summary>
        private void HandleBattleInput()
        {
            Console.WriteLine("\n===== BATTLE ACTION =====");
            Console.WriteLine("1. Attack");
            Console.WriteLine("2. Defend");
            Console.WriteLine("3. Use Skill");
            Console.WriteLine("4. Use Item");
            Console.WriteLine("5. Escape");
            Console.Write("Select an action: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Player uses Attack!");
                    ExecuteAction("Attack");
                    break;
                case "2":
                    Console.WriteLine("Player takes a defensive stance!");
                    ExecuteAction("Defend");
                    break;
                case "3":
                    Console.WriteLine("Opening skill menu...");
                    SelectSkill();
                    break;
                case "4":
                    Console.WriteLine("Opening item menu...");
                    HandleInventoryInput();
                    break;
                case "5":
                    Console.WriteLine("Attempting to escape from battle...");
                    // 도망치기 성공 시 Exploration으로 전환
                    _currentActivity = new Activity(Activity.ActivityType.Exploration);
                    break;
                default:
                    Console.WriteLine("Invalid action. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 모험 중 입력 처리
        /// </summary>
        private void HandleExplorationInput()
        {
            Console.WriteLine("\n===== EXPLORATION MENU =====");
            Console.WriteLine("1. Move to Location");
            Console.WriteLine("2. Check Inventory");
            Console.WriteLine("3. Rest");
            Console.WriteLine("4. Save Game");
            Console.WriteLine("5. Return to Main Menu");
            Console.Write("Select an action: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Where do you want to go?");
                    SelectLocation();
                    break;
                case "2":
                    HandleInventoryInput();
                    break;
                case "3":
                    Console.WriteLine("Resting...");
                    // 플레이어 체력 회복 등의 로직
                    break;
                case "4":
                    Console.WriteLine("Game saved.");
                    break;
                case "5":
                    _currentActivity = null;
                    break;
                default:
                    Console.WriteLine("Invalid action. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 사회 활동(NPC 상호작용, 상점 등) 입력 처리
        /// </summary>
        private void HandleSocialInput()
        {
            Console.WriteLine("\n===== SOCIAL INTERACTION =====");
            Console.WriteLine("1. Talk");
            Console.WriteLine("2. Trade");
            Console.WriteLine("3. Check Inventory");
            Console.WriteLine("4. Accept Quest");
            Console.WriteLine("5. Leave");
            Console.Write("Select an action: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("NPC starts talking...");
                    ExecuteAction("Talk");
                    break;
                case "2":
                    Console.WriteLine("Opening shop...");
                    HandleShop();
                    break;
                case "3":
                    HandleInventoryInput();
                    break;
                case "4":
                    Console.WriteLine("Quest accepted!");
                    ExecuteAction("AcceptQuest");
                    break;
                case "5":
                    _currentActivity = new Activity(Activity.ActivityType.Exploration);
                    break;
                default:
                    Console.WriteLine("Invalid action. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 상점 처리
        /// </summary>
        private void HandleShop()
        {
            Console.WriteLine("\n===== SHOP =====");
            Console.WriteLine("1. Buy Item");
            Console.WriteLine("2. Sell Item");
            Console.WriteLine("3. View Inventory");
            Console.WriteLine("4. Exit Shop");
            Console.Write("Select an action: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("What would you like to buy?");
                    BuyItem();
                    break;
                case "2":
                    Console.WriteLine("What would you like to sell?");
                    SellItem();
                    break;
                case "3":
                    HandleInventoryInput();
                    break;
                case "4":
                    // 돌아가기
                    break;
                default:
                    Console.WriteLine("Invalid action. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 인벤토리 입력 처리
        /// </summary>
        private void HandleInventoryInput()
        {
            Console.WriteLine("\n===== INVENTORY =====");
            Console.WriteLine("1. Use Item");
            Console.WriteLine("2. Drop Item");
            Console.WriteLine("3. Equipment");
            Console.WriteLine("4. Back");
            Console.Write("Select an action: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Which item do you want to use?");
                    UseItem();
                    break;
                case "2":
                    Console.WriteLine("Which item do you want to drop?");
                    DropItem();
                    break;
                case "3":
                    Console.WriteLine("Equipment screen...");
                    break;
                case "4":
                    // 돌아가기 (Activity 상태에서 복귀)
                    break;
                default:
                    Console.WriteLine("Invalid action. Please try again.");
                    break;
            }
        }


        private void ExecuteAction(string actionName)
        {
            TrpgGameLogic.Instance.DoAction(actionName);
        }

        private void SelectLocation()
        {
            Console.WriteLine("1. Dungeon");
            Console.WriteLine("2. Village");
            Console.WriteLine("3. Forest");
            Console.WriteLine("4. Back");
            Console.Write("Select location: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Entering dungeon...");
                    _currentActivity = new Activity(Activity.ActivityType.Combat);
                    break;
                case "2":
                    Console.WriteLine("Arriving at village...");
                    _currentActivity = new Activity(Activity.ActivityType.Social);
                    break;
                case "3":
                    Console.WriteLine("Exploring forest...");
                    // 모험 계속
                    break;
                case "4":
                    // 돌아가기
                    break;
                default:
                    Console.WriteLine("Invalid location. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 스킬 선택
        /// </summary>
        private void SelectSkill()
        {
            Console.WriteLine("Available Skills:");
            Console.WriteLine("1. Fireball");
            Console.WriteLine("2. Heal");
            Console.WriteLine("3. Back");
            Console.Write("Select skill: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Using Fireball!");
                    ExecuteAction("Fireball");
                    break;
                case "2":
                    Console.WriteLine("Using Heal!");
                    ExecuteAction("Heal");
                    break;
                case "3":
                    // 돌아가기
                    break;
                default:
                    Console.WriteLine("Invalid skill. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 아이템 구매
        /// </summary>
        private void BuyItem()
        {
            Console.WriteLine("1. Potion - 50 Gold");
            Console.WriteLine("2. Mana Potion - 80 Gold");
            Console.WriteLine("3. Back");
            Console.Write("Select item: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Bought Potion!");
                    break;
                case "2":
                    Console.WriteLine("Bought Mana Potion!");
                    break;
                case "3":
                    // 돌아가기
                    break;
                default:
                    Console.WriteLine("Invalid item. Please try again.");
                    break;
            }
        }

        /// <summary>
        /// 아이템 판매
        /// </summary>
        private void SellItem()
        {
            if (_currentPlayer == null)
            {
                Console.WriteLine("플레이어가 설정되지 않았습니다.");
                return;
            }

            var equipments = _currentPlayer.playerItemBag.EquipmentItems;

            if (equipments.Count == 0)
            {
                Console.WriteLine("판매할 수 있는 장비가 없습니다.");
                return;
            }

            Console.WriteLine("\n===== 판매 가능한 장비 =====");
            for (int i = 0; i < equipments.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {equipments[i].ItemName} - {equipments[i].ItemDescription}");
            }
            Console.WriteLine($"{equipments.Count + 1}. 돌아가기");
            Console.Write("판매할 장비 선택: ");

            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int selection))
            {
                if (selection == equipments.Count + 1)
                {
                    // 돌아가기
                    return;
                }

                if (selection > 0 && selection <= equipments.Count)
                {
                    int index = selection - 1;
                    var soldItem = equipments[index];
                    _currentPlayer.playerItemBag.DropEquipment(index);
                    Console.WriteLine($"{soldItem.ItemName}을(를) 판매했습니다!");
                    // TODO: 골드 추가 로직 필요
                }
                else
                {
                    Console.WriteLine("잘못된 선택입니다.");
                }
            }
            else
            {
                Console.WriteLine("숫자를 입력해주세요.");
            }
        }

        /// <summary>
        /// 아이템 사용
        /// </summary>
        private void UseItem()
        {
            if (_currentPlayer == null)
            {
                Console.WriteLine("플레이어가 설정되지 않았습니다.");
                return;
            }

            var consumables = _currentPlayer.playerItemBag.ConsumableItems;

            if (consumables.Count == 0)
            {
                Console.WriteLine("사용할 수 있는 아이템이 없습니다.");
                return;
            }

            Console.WriteLine("\n===== 사용 가능한 아이템 =====");
            for (int i = 0; i < consumables.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {consumables[i].ItemName} (x{consumables[i].Quantity}) - {consumables[i].ItemDescription}");
            }
            Console.WriteLine($"{consumables.Count + 1}. 돌아가기");
            Console.Write("사용할 아이템 선택: ");

            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int selection))
            {
                if (selection == consumables.Count + 1)
                {
                    // 돌아가기
                    return;
                }

                if (selection > 0 && selection <= consumables.Count)
                {
                    int index = selection - 1;
                    _currentPlayer.playerItemBag.UseConsumable(index);
                }
                else
                {
                    Console.WriteLine("잘못된 선택입니다.");
                }
            }
            else
            {
                Console.WriteLine("숫자를 입력해주세요.");
            }
        }

        /// <summary>
        /// 아이템 버리기
        /// </summary>
        private void DropItem()
        {
            if (_currentPlayer == null)
            {
                Console.WriteLine("플레이어가 설정되지 않았습니다.");
                return;
            }

            var consumables = _currentPlayer.playerItemBag.ConsumableItems;
            var equipments = _currentPlayer.playerItemBag.EquipmentItems;

            int totalItems = consumables.Count + equipments.Count;

            if (totalItems == 0)
            {
                Console.WriteLine("버릴 수 있는 아이템이 없습니다.");
                return;
            }

            Console.WriteLine("\n===== 인벤토리 =====");
            int displayIndex = 1;

            // 소비 아이템 표시
            if (consumables.Count > 0)
            {
                Console.WriteLine("[소비 아이템]");
                for (int i = 0; i < consumables.Count; i++)
                {
                    Console.WriteLine($"{displayIndex}. {consumables[i].ItemName} (x{consumables[i].Quantity}) - {consumables[i].ItemDescription}");
                    displayIndex++;
                }
            }

            // 장비 아이템 표시
            if (equipments.Count > 0)
            {
                Console.WriteLine("[장비]");
                for (int i = 0; i < equipments.Count; i++)
                {
                    Console.WriteLine($"{displayIndex}. {equipments[i].ItemName} - {equipments[i].ItemDescription}");
                    displayIndex++;
                }
            }

            Console.WriteLine($"{displayIndex}. 돌아가기");
            Console.Write("버릴 아이템 선택: ");

            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int selection))
            {
                if (selection == displayIndex)
                {
                    // 돌아가기
                    return;
                }

                if (selection > 0 && selection < displayIndex)
                {
                    // 소비 아이템 범위인지 확인
                    if (selection <= consumables.Count)
                    {
                        int index = selection - 1;
                        var droppedItem = consumables[index];
                        _currentPlayer.playerItemBag.DropConsumable(index);
                        Console.WriteLine($"{droppedItem.ItemName}을(를) 버렸습니다.");
                    }
                    // 장비 아이템 범위인지 확인
                    else
                    {
                        int index = selection - consumables.Count - 1;
                        var droppedItem = equipments[index];
                        _currentPlayer.playerItemBag.DropEquipment(index);
                        Console.WriteLine($"{droppedItem.ItemName}을(를) 버렸습니다.");
                    }
                }
                else
                {
                    Console.WriteLine("잘못된 선택입니다.");
                }
            }
            else
            {
                Console.WriteLine("숫자를 입력해주세요.");
            }
        }
    }
}