namespace DarkAges.Library.Network;

public class TradeUpdateMessage : NetworkMessage
{
    public TradeAction Action { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public int Gold { get; set; }

    public TradeUpdateMessage()
    {
        MessageType = "TradeUpdate";
    }
}