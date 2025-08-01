namespace DarkAges.Library.Network;

public class ClientBangMessage : NetworkMessage
{
    public string Name { get; set; }
    public string Reason { get; set; }
    public string GmName { get; set; }

    public ClientBangMessage(string name, string reason, string gmName)
    {
        Name = name;
        Reason = reason;
        GmName = gmName;
    }
} 