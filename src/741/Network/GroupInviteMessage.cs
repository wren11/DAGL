namespace DarkAges.Library.Network;

public class GroupInviteMessage : NetworkMessage
{
    public string TargetUsername { get; set; } = "";

    public GroupInviteMessage()
    {
        MessageType = "GroupInvite";
    }
}