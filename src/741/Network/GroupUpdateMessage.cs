namespace DarkAges.Library.Network;

public class GroupUpdateMessage : NetworkMessage
{
    public System.Collections.Generic.List<string> MemberUsernames { get; set; } = [];

    public GroupUpdateMessage()
    {
        MessageType = "GroupUpdate";
    }
}