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
                    // TODO: 저장/로드 시스템이 구현되지 않았습니다.
                    // 필요한 기반 작업: 게임 상태 역직렬화, 세이브 파일 검증, 버전 호환성 처리
                    Console.WriteLine("불러오기 시스템이 아직 구현되지 않았습니다.");
                    break;
                case "3":
                    // TODO: 설정 시스템이 구현되지 않았습니다.
                    // 필요한 기반 작업: 옵션 관리 (음량, 난이도, 키 설정 등)
                    Console.WriteLine("설정 메뉴가 아직 구현되지 않았습니다.");
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
            // TODO: 전투 시스템이 구현되지 않았습니다.
            // 필요한 기반 작업:
            // 1. Battle/Combat 클래스 생성 (전투 상태 관리)
            // 2. Enemy/Monster 클래스 생성
            // 3. 전투 액션 시스템 (Attack, Defend, Skill 사용 로직)
            // 4. 턴제/실시간 전투 시스템 결정
            // 5. 전투 결과 처리 (승리/패배, 경험치, 전리품)

            Console.WriteLine("전투 시스템이 아직 구현되지 않았습니다.");

            // 기존 하드코딩 로직은 제거됨
            // var battleActions = battleSystem.GetAvailableActions(_currentPlayer);
            // ...
        }

        /// <summary>
        /// 모험 중 입력 처리
        /// </summary>
        private void HandleExplorationInput()
        {
            Console.WriteLine("\n===== EXPLORATION MENU =====");
            Console.WriteLine("1. Move to Location");
            Console.WriteLine("2. Use Skill");
            Console.WriteLine("3. Check Inventory");
            Console.WriteLine("4. Rest");
            Console.WriteLine("5. Save Game");
            Console.WriteLine("6. Return to Main Menu");
            Console.Write("Select an action: ");

            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    Console.WriteLine("Where do you want to go?");
                    SelectLocation();
                    break;
                case "2":
                    SelectSkill();
                    break;
                case "3":
                    HandleInventoryInput();
                    break;
                case "4":
                    // TODO: Rest 시스템이 구현되지 않았습니다.
                    // 필요한 기반 작업: HP/MP 회복 로직, 시간 경과 시스템, 안전 지역 확인
                    Console.WriteLine("휴식 시스템이 아직 구현되지 않았습니다.");
                    break;
                case "5":
                    // TODO: 저장/로드 시스템이 구현되지 않았습니다.
                    // 필요한 기반 작업: 게임 상태 직렬화, 파일 I/O, 세이브 슬롯 관리
                    Console.WriteLine("저장 시스템이 아직 구현되지 않았습니다.");
                    break;
                case "6":
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
                    // TODO: NPC 대화 시스템이 구현되지 않았습니다.
                    // 필요한 기반 작업: NPC 클래스, 대화 트리, 선택지 분기, LLM 통합
                    Console.WriteLine("NPC 대화 시스템이 아직 구현되지 않았습니다.");
                    break;
                case "2":
                    Console.WriteLine("Opening shop...");
                    HandleShop();
                    break;
                case "3":
                    HandleInventoryInput();
                    break;
                case "4":
                    // TODO: 퀘스트 수락 시스템이 구현되지 않았습니다.
                    // 필요한 기반 작업: 현재 NPC의 퀘스트 목록 조회, 퀘스트 수락 조건 확인
                    // 참고: Quest 클래스는 이미 TrpgGameLogic.cs에 존재함
                    Console.WriteLine("퀘스트 수락 시스템이 아직 구현되지 않았습니다.");
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
            // TODO: 월드/위치 시스템이 구현되지 않았습니다.
            // 필요한 기반 작업:
            // 1. Location 클래스 생성 (위치 이름, 설명, 접근 가능 여부)
            // 2. World/Map 시스템 (위치 간 연결 관계, 이동 조건)
            // 3. 위치별 Activity 타입 매핑 (Dungeon -> Combat, Village -> Social 등)
            // 4. Chapter/Quest 시스템과 연동 (특정 위치 잠금/해금)

            Console.WriteLine("월드 탐험 시스템이 아직 구현되지 않았습니다.");

            // 기존 하드코딩 로직은 제거됨
            // var availableLocations = worldSystem.GetAccessibleLocations();
            // ...
        }

        /// <summary>
        /// 스킬 선택
        /// </summary>
        private void SelectSkill()
        {
            if (_currentPlayer == null)
            {
                Console.WriteLine("플레이어 정보가 없습니다.");
                return;
            }

            // 탐험 중에는 자기 자신 대상 스킬만 사용 가능
            var selfSkills = _currentPlayer.PlayerSkills
                .FindAll(s => s.TargetType == SkillTargetType.Self);

            if (selfSkills.Count == 0)
            {
                Console.WriteLine("사용 가능한 스킬이 없습니다.");
                return;
            }

            int currentMp = _currentPlayer.CommonAttributes.GetStatus("MP")?.StatusValue ?? 0;
            int currentHp = _currentPlayer.CommonAttributes.GetStatus("HP")?.StatusValue ?? 0;
            Console.WriteLine($"\n===== 스킬 사용 ===== (HP: {currentHp} / MP: {currentMp})");

            for (int i = 0; i < selfSkills.Count; i++)
            {
                var skill = selfSkills[i];
                bool canUse = skill.CanUse(_currentPlayer);
                string label = canUse
                    ? $"{i + 1}. {skill.SkillName} (MP {skill.MpCost}) - {skill.Description}"
                    : $"{i + 1}. {skill.SkillName} (MP 부족)";
                Console.WriteLine(label);
            }
            Console.WriteLine($"{selfSkills.Count + 1}. 돌아가기");
            Console.Write("선택: ");

            string input = Console.ReadLine() ?? "";
            if (int.TryParse(input, out int selected) && selected >= 1 && selected <= selfSkills.Count)
            {
                var skill = selfSkills[selected - 1];
                if (!skill.CanUse(_currentPlayer))
                {
                    Console.WriteLine("MP가 부족합니다!");
                    return;
                }

                var logs = skill.Use(_currentPlayer, _currentPlayer);
                Console.WriteLine($"\n{_currentPlayer.Name}은(는) {skill.SkillName}을(를) 사용했다!");
                foreach (var log in logs)
                {
                    Console.WriteLine(log);
                }
            }
        }

        /// <summary>
        /// 아이템 구매
        /// </summary>
        private void BuyItem()
        {
            // TODO: 상점 시스템이 구현되지 않았습니다.
            // 필요한 기반 작업:
            // 1. Shop 클래스 생성 (상점 아이템 목록, 가격 관리)
            // 2. Player에 Gold/Currency 시스템 추가
            // 3. 상점별 판매 아이템 목록 데이터 구조

            Console.WriteLine("상점 시스템이 아직 구현되지 않았습니다.");

            // 기존 하드코딩 로직은 제거됨
            // if (_currentPlayer == null) { ... }
            // var shopItems = shopSystem.GetAvailableItems();
            // ...
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