using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Macro;

public class SpellEvent(int spellId, SpellEventType type) : Event(EventType.Custom)
{
    public int SpellId { get; set; } = spellId;
    public new SpellEventType Type { get; set; } = type;
}