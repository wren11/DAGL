namespace DarkAges.Library.Network;

public class NpcDialogMessage : NetworkMessage
{
    public int NpcId { get; set; }
    public string DialogText { get; set; } = "";
    public System.Collections.Generic.List<string> Options { get; set; } = [];

    public NpcDialogMessage()
    {
        MessageType = "NpcDialog";
    }
}