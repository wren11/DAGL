using System;

namespace DarkAges.Library.Network;

public class NetworkErrorEventArgs : EventArgs
{
    public NetworkError Error { get; }

    public NetworkErrorEventArgs(NetworkError error)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }
} 