namespace DarkAges.Library.UI.Chat;

public class ChatMessage
{
    public string Text { get; set; }
    public ChatType Type { get; set; }
    public string Sender { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsHighlighted { get; set; }
    public int MessageId { get; set; }
}