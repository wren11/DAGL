namespace DarkAges.Library.Network;

public class ChatMessage : NetworkMessage
{
    public string SenderName { get; set; } = "";
    public string Content { get; set; } = "";
    public ChatType Type { get; set; }

    public ChatMessage()
    {
        MessageType = "Chat";
    }
}