using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Macro;

public class ChatEvent(string message, ChatType type) : Event(EventType.Custom)
{
    public string Message { get; set; } = message;
    public new ChatType Type { get; set; } = type;
}