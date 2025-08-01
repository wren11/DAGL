namespace DarkAges.Library.Network;

public class GroupLeaveMessage : NetworkMessage
{
    public GroupLeaveMessage()
    {
        MessageType = "GroupLeave";
    }
}