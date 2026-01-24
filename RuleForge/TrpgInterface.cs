using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class TrpgInterface
    {
        private static TrpgInterface? _instance;
        private Activity? _currentActivity;

        public Activity? CurrentActivity
        {
            get { return _currentActivity; }
            set { _currentActivity = value; }
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

        /// <summary>
        /// 게임 활동 타입에 따라 사용자 입력을 처리하는 메소드
        /// </summary>
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

        /// <summary>
        /// 메인 메뉴 입력 처리
        /// </summary>
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
                    TrpgGameLogic.Instance.StartGame();
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

        // ============ 도우미 메소드들 ============

        /// <summary>
        /// 액션 실행 (전투, 대화 등)
        /// </summary>
        private void ExecuteAction(string actionName)
        {
            Console.WriteLine($"Executing action: {actionName}");
            // 실제 액션 로직은 TrpgGameLogic에 위임
        }

        /// <summary>
        /// 위치 선택
        /// </summary>
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
            Console.WriteLine("1. Iron Sword - 200 Gold");
            Console.WriteLine("2. Leather Armor - 150 Gold");
            Console.WriteLine("3. Back");
            Console.Write("Select item to sell: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Sold Iron Sword!");
                    break;
                case "2":
                    Console.WriteLine("Sold Leather Armor!");
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
        /// 아이템 사용
        /// </summary>
        private void UseItem()
        {
            Console.WriteLine("1. Potion");
            Console.WriteLine("2. Mana Potion");
            Console.WriteLine("3. Back");
            Console.Write("Select item to use: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Used Potion! HP recovered.");
                    break;
                case "2":
                    Console.WriteLine("Used Mana Potion! Mana recovered.");
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
        /// 아이템 버리기
        /// </summary>
        private void DropItem()
        {
            Console.WriteLine("1. Damaged Shield");
            Console.WriteLine("2. Old Map");
            Console.WriteLine("3. Back");
            Console.Write("Select item to drop: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Dropped Damaged Shield.");
                    break;
                case "2":
                    Console.WriteLine("Dropped Old Map.");
                    break;
                case "3":
                    // 돌아가기
                    break;
                default:
                    Console.WriteLine("Invalid item. Please try again.");
                    break;
            }
        }
    }
}