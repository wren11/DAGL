namespace DarkAges.Library.Network;

public class PongMessage : NetworkMessage
{
    public DateTime OriginalTimestamp { get; set; }

    public PongMessage()
    {
        MessageType = "Pong";
    }
}