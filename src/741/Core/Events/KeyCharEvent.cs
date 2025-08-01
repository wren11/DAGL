namespace DarkAges.Library.Core.Events;

public class KeyCharEvent : Event
{
    public char Char { get; }

    public KeyCharEvent(char c) : base(EventType.KeyChar)
    {
        Char = c;
    }
} 