namespace DarkAges.Library.Network;

public class TradeStartMessage : NetworkMessage
{
    public string PartnerUsername { get; set; } = "";

    public TradeStartMessage()
    {
        MessageType = "TradeStart";
    }
}