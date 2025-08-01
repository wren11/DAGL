using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Reconnect;

public class ConnectionEvent(string serverAddress, int port, ConnectionEventType type)
        : Event(EventType.ConnectionEvent)
{
    public string ServerAddress { get; set; } = serverAddress;
    public int Port { get; set; } = port;
    public new ConnectionEventType Type { get; set; } = type;
}