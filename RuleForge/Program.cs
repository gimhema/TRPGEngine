using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LLama.Native;

namespace RuleForge;

internal static class Program
{
    public enum GameMode
    {
        DEFAULT = 0,
        SINGLE = 1,
        MULTI_HOST = 2,
        MULTI_CLIENT = 3
    }

    public static GameMode CurrentGameMode = GameMode.DEFAULT;
    private static LlamaEngine? _llamaEngine;

    public static void SelectGameMode(GameMode mode)
    {
        CurrentGameMode = mode;
    }

    public static async Task StartGameSingle(string[] args)
    {
        var controller = TrpgGameController.Instance;
        controller.Initialize();

        var gameState = controller.GetGameState();
        TrpgGameLogic.Instance.InitializeGame(gameState);

        await controller.RunAsync();
    }

    public static async Task StartGameMultiHost(string[] args)
    {
        var ip = IPAddress.Any;
        var port = 7777;

        if (args.Length >= 1 && int.TryParse(args[0], out var p)) port = p;

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var server = new TcpServer(
            new IPEndPoint(ip, port),
            new TcpServerOptions
            {
                Backlog = 512,
                MaxConnections = 10_000,
                MaxMessageBytes = 1 * 1024 * 1024, // 1MB
                ReceiveBufferSize = 64 * 1024,
                SendBufferSize = 64 * 1024,
                NoDelay = true,
                KeepAlive = true,
            });

        server.OnMessage = async (conn, payload, ct) =>
        {
            var text = Encoding.UTF8.GetString(payload.Span);
            Console.WriteLine($"[{conn.Id}] {text}");

            var reply = Encoding.UTF8.GetBytes("OK: " + text);
            await conn.SendAsync(reply, ct);
        };

        Console.WriteLine($"Listening on {ip}:{port}  (Ctrl+C to stop)");

        // TODO: TCP 서버와 GameController 통합 필요
        var controller = TrpgGameController.Instance;
        controller.Initialize();
        var gameState = controller.GetGameState();
        TrpgGameLogic.Instance.InitializeGame(gameState);

        // 서버와 게임 루프를 동시에 실행
        var serverTask = server.RunAsync(cts.Token);
        var gameTask = controller.RunAsync(cts.Token);

        await Task.WhenAny(serverTask, gameTask);
        Console.WriteLine("Server or game stopped.");
    }

    public static async Task GameStart(string[] args)
    {
        switch(CurrentGameMode)
        {
            case GameMode.SINGLE:
                // Start single-player game logic
                await StartGameSingle(args);
                break;
            case GameMode.MULTI_HOST:
                // Start multi-player host game logic
                await StartGameMultiHost(args);
                break;
            case GameMode.MULTI_CLIENT:
                // Start multi-player client game logic
                Console.WriteLine("Multi-player client mode not implemented yet.");
                break;
            default:
                throw new InvalidOperationException("Game mode not selected.");
        }
    }

    public static void GameStartPreprocess()
    {
        // 룰북 JSON 파일 파싱 및 게임 시스템 초기화 (아이템/적/스킬/월드)
        TrpgGameLogic.Instance.LoadRulebook();

        // LLamaSharp 네이티브 로그 억제 (모델 로드 전 설정 필수)
        NativeLibraryConfig.All.WithLogCallback((NativeLogConfig.LLamaLogCallback?)null);

        // GameSetting.ini에서 모델 경로 로드
        var setting = new GameSetting();
        setting.LoadINI();
        var modelPath = setting.ModelPath;

        if (!string.IsNullOrWhiteSpace(modelPath) && System.IO.File.Exists(modelPath))
        {
            try
            {
                _llamaEngine = ConsoleSpinner.Run("LLM 모델 로딩 중...", () =>
                {
                    var engine = new LlamaEngine(modelPath);
                    NpcDialogueManager.Instance.SetEngine(engine);
                    NarratorManager.Instance.SetEngine(engine);
                    return engine;
                });
                Console.WriteLine("LLM 모델 로딩 완료.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LLM 모델 로딩 실패, 폴백 모드로 실행합니다. ({ex.Message})");
            }
        }
        else
        {
            Console.WriteLine("LLM 모델 없음 — 폴백 대화 모드로 실행합니다.");
        }
    }

    public static async Task Main(string[] args)
    {
        GameStartPreprocess();

        Console.WriteLine("Select Game Mode: 1) Single Player  2) Multi Player Host  3) Multi Player Client");
        var input = Console.ReadLine() ?? "";

        switch (input)
        {
            case "1":
                SelectGameMode(GameMode.SINGLE);
                break;
            case "2":
                SelectGameMode(GameMode.MULTI_HOST);
                break;
            case "3":
                SelectGameMode(GameMode.MULTI_CLIENT);
                break;
            default:
                Console.WriteLine("Invalid selection. Exiting.");
                return;
        }

        await GameStart(args);

        // LLamaWeights 해제 전, Context를 가진 세션들을 먼저 정리
        TrpgGameLogic.Instance.WorldManager.DisposeAllNpcSessions();
        NarratorManager.Instance.Dispose();
        _llamaEngine?.Dispose();
    }
}