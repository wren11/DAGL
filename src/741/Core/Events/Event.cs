using System;

namespace DarkAges.Library.Core.Events;

public abstract class Event(EventType type)
{
    public EventType Type { get; set; } = type;
    public bool Handled { get; set; } = false;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    protected Event() : this(EventType.None)
    {
    }
}