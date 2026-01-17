using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    public static void SelectGameMode(GameMode mode)
    {
        CurrentGameMode = mode;
    }

    public static async void StartGameSingle(string[] args)
    {
        TrpgGameLogic.Instance.StartGame();
    }

    public static async void StartGameMultiHost(string[] args)
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
        await server.RunAsync(cts.Token);
        Console.WriteLine("Server stopped.");

        TrpgGameLogic.Instance.StartGame();
    }

    public static void GameStart(string[] args)
    {
        switch(CurrentGameMode)
        {
            case GameMode.SINGLE:
                // Start single-player game logic
                StartGameSingle(args);
                break;
            case GameMode.MULTI_HOST:
                // Start multi-player host game logic
                StartGameMultiHost(args);
                break;
            case GameMode.MULTI_CLIENT:
                // Start multi-player client game logic
                break;
            default:
                throw new InvalidOperationException("Game mode not selected.");
        }
    }

    public static void GameStartPreprocess()
    {
        // Load LLM Models . . .

        // Setting TRPG Rule . . .
    }

    public static async Task Main(string[] args)
    {

        GameStartPreprocess();

        Console.WriteLine("Select Game Mode: 1) Single Player  2) Multi Player Host  3) Multi Player Client");
        var input = Console.ReadLine();
        switch (input)
        {
            case "1":
                SelectGameMode(GameMode.SINGLE);
                StartGameSingle(args);
                break;
            case "2":
                SelectGameMode(GameMode.MULTI_HOST);
                StartGameMultiHost(args);
                break;
            case "3":
                SelectGameMode(GameMode.MULTI_CLIENT);
                // StartGameMultiClient(args); // Implement this method as needed
                break;
            default:
                Console.WriteLine("Invalid selection. Exiting.");
                break;
        }

        GameStart(args);


    }
}