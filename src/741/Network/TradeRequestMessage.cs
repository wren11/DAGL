namespace DarkAges.Library.Network;

public class TradeRequestMessage : NetworkMessage
{
    public string TargetUsername { get; set; } = "";

    public TradeRequestMessage()
    {
        MessageType = "TradeRequest";
    }
}