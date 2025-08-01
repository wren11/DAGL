namespace DarkAges.Library.Network;

public class DisconnectMessage : NetworkMessage
{
    public string Reason { get; set; } = "";

    public DisconnectMessage()
    {
        MessageType = "Disconnect";
    }
}