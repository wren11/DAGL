namespace DarkAges.Library.Network;

public class PingMessage : NetworkMessage
{
    public PingMessage()
    {
        MessageType = "Ping";
    }
}