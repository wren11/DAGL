using System;

namespace DarkAges.Library.Network;

public class SocketDataEventArgs : EventArgs
{
    public byte[] Data { get; }

    public SocketDataEventArgs(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}