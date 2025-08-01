namespace DarkAges.Library.Network;

public class SocketErrorEventArgs(NetworkSocket socket, Exception error) : SocketEventArgs($"Socket error on {socket?.ToString() ?? "null"}: {error?.Message ?? "Unknown error"}")
{
    public NetworkSocket Socket { get; } = socket ?? throw new ArgumentNullException(nameof(socket));
    public Exception Error { get; } = error ?? throw new ArgumentNullException(nameof(error));
}