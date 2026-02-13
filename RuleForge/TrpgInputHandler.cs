using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuleForge
{
    /// <summary>
    /// 사용자 입력을 처리하는 클래스
    /// 게임 로직과 분리되어 순수하게 입력만 담당
    /// </summary>
    public class TrpgInputHandler
    {
        private static TrpgInputHandler? _instance;

        public static TrpgInputHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TrpgInputHandler();
                }
                return _instance;
            }
        }

        private TrpgInputHandler()
        {
        }

        /// <summary>
        /// 동기 방식으로 사용자 입력 읽기
        /// </summary>
        public string ReadInput()
        {
            return Console.ReadLine() ?? "";
        }

        /// <summary>
        /// 비동기 방식으로 사용자 입력 읽기
        /// </summary>
        public async Task<string> ReadInputAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                while (!Console.KeyAvailable)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return "";
                    }
                    Thread.Sleep(50);
                }
                return Console.ReadLine() ?? "";
            }, cancellationToken);
        }

        /// <summary>
        /// 입력이 특수 명령인지 확인 (/ 로 시작하는 명령)
        /// </summary>
        public bool IsSpecialCommand(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.StartsWith("/");
        }

        /// <summary>
        /// 특수 명령 처리
        /// </summary>
        /// <returns>명령이 처리되었으면 true, 아니면 false</returns>
        public bool HandleSpecialCommand(string input, TrpgGameState state)
        {
            if (!IsSpecialCommand(input))
            {
                return false;
            }

            string command = input.ToLower().Trim();

            switch (command)
            {
                case "/quit":
                case "/exit":
                    HandleQuit(state);
                    return true;

                case "/home":
                case "/menu":
                    HandleHome(state);
                    return true;

                case "/settings":
                case "/config":
                    HandleSettings(state);
                    return true;

                case "/help":
                    HandleHelp();
                    return true;

                case "/back":
                    HandleBack(state);
                    return true;

                case "/status":
                    HandleStatus(state);
                    return true;

                default:
                    Console.WriteLine($"알 수 없는 명령어: {input}");
                    Console.WriteLine("사용 가능한 명령어를 보려면 /help를 입력하세요.");
                    return true;
            }
        }

        /// <summary>
        /// 게임 종료 명령
        /// </summary>
        private void HandleQuit(TrpgGameState state)
        {
            Console.WriteLine("\n정말로 종료하시겠습니까? (y/n)");
            string confirmation = Console.ReadLine()?.ToLower() ?? "";

            if (confirmation == "y" || confirmation == "yes")
            {
                Console.WriteLine("게임을 종료합니다...");
                state.ShouldExit = true;
            }
            else
            {
                Console.WriteLine("종료를 취소했습니다.");
            }
        }

        /// <summary>
        /// 메인 메뉴로 복귀
        /// </summary>
        private void HandleHome(TrpgGameState state)
        {
            Console.WriteLine("\n메인 메뉴로 돌아갑니다...");
            state.ChangeScene(TrpgGameState.SceneType.MainMenu);
        }

        /// <summary>
        /// 설정 화면으로 이동
        /// </summary>
        private void HandleSettings(TrpgGameState state)
        {
            Console.WriteLine("\n설정 화면으로 이동합니다...");
            state.ChangeScene(TrpgGameState.SceneType.Settings);
        }

        /// <summary>
        /// 도움말 표시
        /// </summary>
        private void HandleHelp()
        {
            Console.WriteLine("\n========== 사용 가능한 명령어 ==========");
            Console.WriteLine("/quit, /exit    - 게임 종료");
            Console.WriteLine("/home, /menu    - 메인 메뉴로 복귀");
            Console.WriteLine("/settings       - 설정 화면");
            Console.WriteLine("/back           - 이전 화면으로");
            Console.WriteLine("/status         - 현재 상태 확인");
            Console.WriteLine("/help           - 이 도움말 표시");
            Console.WriteLine("=========================================\n");
        }

        /// <summary>
        /// 이전 씬으로 복귀
        /// </summary>
        private void HandleBack(TrpgGameState state)
        {
            if (state.PreviousScene.HasValue)
            {
                Console.WriteLine("\n이전 화면으로 돌아갑니다...");
                state.ReturnToPreviousScene();
            }
            else
            {
                Console.WriteLine("\n이전 화면이 없습니다.");
            }
        }

        /// <summary>
        /// 현재 게임 상태 표시
        /// </summary>
        private void HandleStatus(TrpgGameState state)
        {
            Console.WriteLine("\n========== 현재 상태 ==========");
            Console.WriteLine($"현재 씬: {state.CurrentScene}");

            if (state.CurrentPlayer != null)
            {
                Console.WriteLine($"플레이어: {state.CurrentPlayer.Name}");
                Console.WriteLine($"레벨: {state.CurrentPlayer.playerProfile.PlayerLevel}");
                Console.WriteLine($"직업: {state.CurrentPlayer.playerProfile.job}");
            }

            if (state.CurrentChapter != null)
            {
                Console.WriteLine($"챕터: {state.CurrentChapter.Title}");
            }

            if (state.CurrentQuest != null)
            {
                Console.WriteLine($"퀘스트: {state.CurrentQuest.Title}");
            }

            if (state.CurrentLocation != null)
            {
                Console.WriteLine($"현재 위치: {state.CurrentLocation.GetName()}");
            }

            Console.WriteLine("================================\n");
        }

        /// <summary>
        /// 입력 유효성 검사
        /// </summary>
        public bool IsValidInput(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// 숫자 입력인지 확인
        /// </summary>
        public bool IsNumericInput(string input, out int number)
        {
            return int.TryParse(input.Trim(), out number);
        }

        /// <summary>
        /// 선택지 ID로 선택 찾기 (숫자 또는 문자열)
        /// </summary>
        public TrpgChoice? FindChoiceByInput(string input, TrpgGameState state)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            string trimmedInput = input.Trim();

            // 직접 ID로 검색
            var choice = state.FindChoice(trimmedInput);
            if (choice != null)
            {
                return choice;
            }

            // 숫자로 변환해서 검색 (1-based index를 ID로 사용하는 경우)
            if (IsNumericInput(trimmedInput, out int index))
            {
                choice = state.FindChoice(index.ToString());
                if (choice != null)
                {
                    return choice;
                }
            }

            return null;
        }
    }
}
