namespace DarkAges.Library.Network;

public class NpcInteractMessage : NetworkMessage
{
    public int NpcId { get; set; }

    public NpcInteractMessage()
    {
        MessageType = "NpcInteract";
    }
}