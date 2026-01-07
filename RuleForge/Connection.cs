using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTcpServer;

public sealed class Connection : IDisposable
{
    private readonly Socket _socket;
    private readonly TcpServerOptions _opt;

    private readonly Func<Connection, ReadOnlyMemory<byte>, CancellationToken, Task>? _onMessage;

    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private bool _disposed;

    public long Id { get; }

    public Connection(
        long id,
        Socket socket,
        TcpServerOptions options,
        Func<Connection, ReadOnlyMemory<byte>, CancellationToken, Task>? onMessage)
    {
        Id = id;
        _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _opt = options ?? throw new ArgumentNullException(nameof(options));
        _onMessage = onMessage;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        // length-prefixed: [u32 big-endian length][payload]
        var header = new byte[4];

        while (!ct.IsCancellationRequested)
        {
            // 1) header(4) 읽기
            var ok = await SocketIO.ReadExactlyAsync(_socket, header, ct);
            if (!ok) break;

            var len = Protocol.ReadU32BE(header);
            if (len == 0) continue;

            if (len > (uint)_opt.MaxMessageBytes)
            {
                // 비정상 큰 패킷 차단
                break;
            }

            // 2) payload 읽기 (ArrayPool)
            var rented = ArrayPool<byte>.Shared.Rent((int)len);
            try
            {
                var payloadMem = rented.AsMemory(0, (int)len);

                ok = await SocketIO.ReadExactlyAsync(_socket, payloadMem, ct);
                if (!ok) break;

                if (_onMessage != null)
                {
                    await _onMessage(this, payloadMem, ct);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    /// <summary>
    /// payload를 length-prefixed로 전송.
    /// </summary>
    public async Task SendAsync(ReadOnlyMemory<byte> payload, CancellationToken ct)
    {
        if (payload.Length > _opt.MaxMessageBytes)
            throw new ArgumentOutOfRangeException(nameof(payload), "payload too large");

        // 멀티 스레드 Send interleave 방지
        await _sendLock.WaitAsync(ct);
        try
        {
            Span<byte> header = stackalloc byte[4];
            Protocol.WriteU32BE(header, (uint)payload.Length);

            await SocketIO.WriteAllAsync(_socket, header.ToArray(), ct);
            await SocketIO.WriteAllAsync(_socket, payload, ct);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try { _socket.Shutdown(SocketShutdown.Both); } catch { /* ignore */ }
        try { _socket.Close(); } catch { /* ignore */ }
        _socket.Dispose();

        _sendLock.Dispose();
    }
}
