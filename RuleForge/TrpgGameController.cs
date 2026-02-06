using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuleForge
{
    /// <summary>
    /// 게임의 핵심 컨트롤러
    /// 입력, 로직, 상태, 렌더링을 통합 관리
    /// </summary>
    public class TrpgGameController
    {
        private static TrpgGameController? _instance;

        public static TrpgGameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TrpgGameController();
                }
                return _instance;
            }
        }

        private TrpgGameState _gameState;
        private TrpgInputHandler _inputHandler;
        private TrpgRenderer _renderer;
        private TrpgGameLogic _gameLogic;

        private bool _isRunning;

        private TrpgGameController()
        {
            _gameState = new TrpgGameState();
            _inputHandler = TrpgInputHandler.Instance;
            _renderer = TrpgRenderer.Instance;
            _gameLogic = TrpgGameLogic.Instance;
            _isRunning = false;
        }

        /// <summary>
        /// 게임 컨트롤러 초기화
        /// </summary>
        public void Initialize()
        {
            _gameState = new TrpgGameState();
            _gameState.CurrentScene = TrpgGameState.SceneType.MainMenu;
        }

        /// <summary>
        /// 메인 게임 루프 실행
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _isRunning = true;

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                // 1. 화면 렌더링
                _renderer.Render(_gameState);

                // 2. 종료 조건 체크
                if (_gameState.ShouldExit)
                {
                    _renderer.RenderMessage("\n게임을 종료합니다.");
                    break;
                }

                // 3. 사용자 입력 대기
                Console.Write("\n입력: ");
                string input = _inputHandler.ReadInput();

                // 4. 입력 유효성 검사
                if (!_inputHandler.IsValidInput(input))
                {
                    continue;
                }

                // 5. 특수 명령 처리
                if (_inputHandler.IsSpecialCommand(input))
                {
                    _inputHandler.HandleSpecialCommand(input, _gameState);
                    continue;
                }

                // 6. 일반 입력 처리
                ProcessInput(input);

                // 약간의 딜레이 (CPU 부하 방지)
                await Task.Delay(50, cancellationToken);
            }

            _isRunning = false;
        }

        /// <summary>
        /// 일반 입력 처리
        /// </summary>
        private void ProcessInput(string input)
        {
            // 선택지에서 입력에 해당하는 선택 찾기
            var choice = _inputHandler.FindChoiceByInput(input, _gameState);

            if (choice != null)
            {
                if (choice.IsEnabled)
                {
                    // 선택지 실행
                    choice.Execute(_gameState);
                }
                else
                {
                    _renderer.RenderWarning("이 선택지는 현재 사용할 수 없습니다.");
                }
            }
            else
            {
                // 선택지가 없으면 게임 로직으로 전달
                _gameLogic.ProcessInput(input, _gameState);
            }
        }

        /// <summary>
        /// 게임 상태 가져오기
        /// </summary>
        public TrpgGameState GetGameState()
        {
            return _gameState;
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _gameState.ShouldExit = true;
        }
    }
}
