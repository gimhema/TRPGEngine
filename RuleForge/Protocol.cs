using System;

namespace AsyncTcpServer;

public static class Protocol
{
    public static uint ReadU32BE(ReadOnlySpan<byte> b)
    {
        // big-endian
        return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
    }

    public static void WriteU32BE(Span<byte> dst4, uint v)
    {
        dst4[0] = (byte)((v >> 24) & 0xFF);
        dst4[1] = (byte)((v >> 16) & 0xFF);
        dst4[2] = (byte)((v >> 8) & 0xFF);
        dst4[3] = (byte)(v & 0xFF);
    }
}
