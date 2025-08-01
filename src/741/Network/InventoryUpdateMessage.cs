namespace DarkAges.Library.Network;

public class InventoryUpdateMessage : NetworkMessage
{
    public System.Collections.Generic.List<InventoryItem> Items { get; set; } = [];

    public InventoryUpdateMessage()
    {
        MessageType = "InventoryUpdate";
    }
}