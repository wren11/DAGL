namespace DarkAges.Library.Network;

public class MapDataMessage : NetworkMessage
{
    public string MapName { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] TileData { get; set; } = [];

    public MapDataMessage()
    {
        MessageType = "MapData";
    }
}