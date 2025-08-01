namespace DarkAges.Library.Network;

public class TradeResponseMessage : NetworkMessage
{
    public string RequestingUsername { get; set; } = "";
    public bool Accepted { get; set; }

    public TradeResponseMessage()
    {
        MessageType = "TradeResponse";
    }
}