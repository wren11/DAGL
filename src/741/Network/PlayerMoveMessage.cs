namespace DarkAges.Library.Network;

public class PlayerMoveMessage : NetworkMessage
{
    public float NewX { get; set; }
    public float NewY { get; set; }
    public byte Direction { get; set; }

    public PlayerMoveMessage()
    {
        MessageType = "PlayerMove";
    }
}