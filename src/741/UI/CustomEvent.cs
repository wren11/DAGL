namespace DarkAges.Library.UI;

/// <summary>
/// Custom event class for dialog events
/// </summary>
public class CustomEvent(DarkAges.Library.Core.Events.EventType type, object data)
        : DarkAges.Library.Core.Events.Event(type)
{
    public object Data { get; set; } = data;
}