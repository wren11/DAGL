namespace DarkAges.Library.Network;

public class ObjectSpawnMessage : NetworkMessage
{
    public int ObjectId { get; set; }
    public string ObjectType { get; set; } = ""; // "Player", "NPC", "Monster", "Item"
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public object? SpawnData { get; set; } // e.g., PlayerAppearance, NpcInfo

    public ObjectSpawnMessage()
    {
        MessageType = "ObjectSpawn";
    }
}