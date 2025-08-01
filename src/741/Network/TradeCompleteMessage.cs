namespace DarkAges.Library.Network;

public class TradeCompleteMessage : NetworkMessage
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";

    public TradeCompleteMessage()
    {
        MessageType = "TradeComplete";
    }
}