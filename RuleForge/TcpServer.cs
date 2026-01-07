using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTcpServer;

public sealed class TcpServer
{
    private readonly IPEndPoint _bindEndPoint;
    private readonly TcpServerOptions _opt;

    private Socket? _listenSocket;
    private long _connIdSeq = 0;

    private readonly ConcurrentDictionary<long, Connection> _connections = new();

    public TcpServer(IPEndPoint bindEndPoint, TcpServerOptions options)
    {
        _bindEndPoint = bindEndPoint;
        _opt = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// payload = "프레임(길이 프리픽스 제거된 바디)".
    /// </summary>
    public Func<Connection, ReadOnlyMemory<byte>, CancellationToken, Task>? OnMessage { get; set; }

    public async Task RunAsync(CancellationToken ct)
    {
        _listenSocket = new Socket(_bindEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // 서버 재시작 편의 (Windows/Linux 모두)
        _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        _listenSocket.Bind(_bindEndPoint);
        _listenSocket.Listen(_opt.Backlog);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                Socket client;
                try
                {
                    client = await _listenSocket.AcceptAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (_connections.Count >= _opt.MaxConnections)
                {
                    try { client.Close(); } catch { /* ignore */ }
                    continue;
                }

                ConfigureClientSocket(client);

                var id = Interlocked.Increment(ref _connIdSeq);
                var conn = new Connection(id, client, _opt, OnMessage);

                if (!_connections.TryAdd(id, conn))
                {
                    conn.Dispose();
                    continue;
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await conn.RunAsync(ct);
                    }
                    catch
                    {
                        // 필요시 로깅
                    }
                    finally
                    {
                        _connections.TryRemove(id, out _);
                        conn.Dispose();
                    }
                }, CancellationToken.None);
            }
        }
        finally
        {
            // 신규 수락 중단
            try { _listenSocket.Close(); } catch { /* ignore */ }

            // 기존 커넥션 정리
            foreach (var kv in _connections)
            {
                try { kv.Value.Dispose(); } catch { /* ignore */ }
            }
            _connections.Clear();
        }
    }

    private void ConfigureClientSocket(Socket s)
    {
        s.NoDelay = _opt.NoDelay;
        s.ReceiveBufferSize = _opt.ReceiveBufferSize;
        s.SendBufferSize = _opt.SendBufferSize;

        if (_opt.KeepAlive)
        {
            try
            {
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }
            catch
            {
                // 플랫폼/권한에 따라 실패할 수 있으니 무시 가능
            }
        }
    }
}

public sealed class TcpServerOptions
{
    public int Backlog { get; init; } = 512;
    public int MaxConnections { get; init; } = 10_000;

    public int MaxMessageBytes { get; init; } = 1 * 1024 * 1024;

    public int ReceiveBufferSize { get; init; } = 64 * 1024;
    public int SendBufferSize { get; init; } = 64 * 1024;

    public bool NoDelay { get; init; } = true;
    public bool KeepAlive { get; init; } = true;
}
