namespace DarkAges.Library.Network;

public class WorldUpdateMessage : NetworkMessage
{
    public string UpdateType { get; set; } = "";
    public object? UpdateData { get; set; }

    public WorldUpdateMessage()
    {
        MessageType = "WorldUpdate";
    }
}