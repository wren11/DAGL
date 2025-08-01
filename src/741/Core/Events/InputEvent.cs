namespace DarkAges.Library.Core.Events;

public class InputEvent : Event
{
    public string Text { get; set; } = "";

    public InputEvent(string text) : base(EventType.None)
    {
        Text = text;
    }

    public InputEvent(EventType type, string text) : base(type)
    {
        Text = text;
    }
}