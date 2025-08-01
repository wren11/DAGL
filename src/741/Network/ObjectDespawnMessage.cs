namespace DarkAges.Library.Network;

public class ObjectDespawnMessage : NetworkMessage
{
    public int ObjectId { get; set; }

    public ObjectDespawnMessage()
    {
        MessageType = "ObjectDespawn";
    }
}