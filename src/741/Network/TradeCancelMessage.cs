namespace DarkAges.Library.Network;

public class TradeCancelMessage : NetworkMessage
{
    public string Reason { get; set; } = "";
        
    public TradeCancelMessage()
    {
        MessageType = "TradeCancel";
    }
}