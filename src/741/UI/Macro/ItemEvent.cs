using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Macro;

public class ItemEvent(int itemId, ItemEventType type) : Event(EventType.Custom)
{
    public int ItemId { get; set; } = itemId;
    public new ItemEventType Type { get; set; } = type;
}