namespace DarkAges.Library.UI.Chat;

public class ChatMessageEventArgs(ChatMessage message) : EventArgs
{
    public ChatMessage Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
}