using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTcpServer;

public static class SocketIO
{
    /// <summary>
    /// 정확히 buffer.Length 만큼 읽는다.
    /// 연결 종료면 false.
    /// </summary>
    public static async ValueTask<bool> ReadExactlyAsync(Socket s, byte[] buffer, CancellationToken ct)
        => await ReadExactlyAsync(s, buffer.AsMemory(), ct);

    public static async ValueTask<bool> ReadExactlyAsync(Socket s, Memory<byte> buffer, CancellationToken ct)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            int n;
            try
            {
                n = await s.ReceiveAsync(buffer.Slice(offset), SocketFlags.None, ct);
            }
            catch (OperationCanceledException) { return false; }
            catch (SocketException) { return false; }

            if (n == 0) return false; // remote closed
            offset += n;
        }
        return true;
    }

    /// <summary>
    /// 전체를 쓸 때까지 반복.
    /// </summary>
    public static async ValueTask WriteAllAsync(Socket s, ReadOnlyMemory<byte> data, CancellationToken ct)
    {
        var offset = 0;
        while (offset < data.Length)
        {
            int n;
            try
            {
                n = await s.SendAsync(data.Slice(offset), SocketFlags.None, ct);
            }
            catch (OperationCanceledException) { return; }
            catch (SocketException) { return; }

            if (n <= 0) return;
            offset += n;
        }
    }
}
