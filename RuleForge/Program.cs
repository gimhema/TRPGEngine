using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTcpServer;

internal static class Program
{
    public static async Task Main(string[] args)
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
    }
}