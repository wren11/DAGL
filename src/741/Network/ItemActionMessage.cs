namespace DarkAges.Library.Network;

public class ItemActionMessage : NetworkMessage
{
    public ItemActionType ActionType { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; } = 1;

    public ItemActionMessage()
    {
        MessageType = "ItemAction";
    }
}