namespace DarkAges.Library.Network;

public class SocketManager : IDisposable
{
    private readonly List<NetworkSocket> _sockets = [];
    private readonly object _lockObject = new object();

    public event EventHandler<SocketEventArgs> SocketConnected;
    public event EventHandler<SocketEventArgs> SocketDisconnected;
    public event EventHandler<SocketDataEventArgs> SocketDataReceived;
    public event EventHandler<SocketErrorEventArgs> SocketErrorOccurred;

    public NetworkSocket CreateSocket()
    {
        var socket = new NetworkSocket();
        socket.Connected += (s, e) => SocketConnected?.Invoke(this, e);
        socket.Disconnected += (s, e) => SocketDisconnected?.Invoke(this, e);
        socket.DataReceived += (s, e) => SocketDataReceived?.Invoke(this, e);
        socket.ErrorOccurred += (s, e) => SocketErrorOccurred?.Invoke(this, e);

        lock (_lockObject)
        {
            _sockets.Add(socket);
        }

        return socket;
    }

    public void RemoveSocket(NetworkSocket socket)
    {
        if (socket == null)
            return;

        lock (_lockObject)
        {
            _sockets.Remove(socket);
        }
    }

    public void DisconnectAll()
    {
        lock (_lockObject)
        {
            foreach (var socket in _sockets)
            {
                try
                {
                    socket.Disconnect();
                }
                catch
                {
                }
            }
        }
    }

    public void Dispose()
    {
        DisconnectAll();
        lock (_lockObject)
        {
            foreach (var socket in _sockets)
            {
                socket.Dispose();
            }
            _sockets.Clear();
        }
    }
}