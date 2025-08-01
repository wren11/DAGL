using System;

namespace DarkAges.Library.Network;

public class SocketEventArgs : EventArgs
{
    public string Message { get; }

    public SocketEventArgs(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}