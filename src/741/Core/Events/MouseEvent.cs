namespace DarkAges.Library.Core.Events;

public class MouseEvent : Event
{
    public int X { get; set; }
    public int Y { get; set; }
    public MouseButton Button { get; set; }
    public int Delta { get; set; }
    public MouseButtonFlags ButtonFlags { get; set; }
    public bool IsDoubleClick { get; set; }

    public MouseEvent(EventType type, int x, int y, MouseButton button = (MouseButton)(-1), int delta = 0)
            : base(type)
    {
        X = x;
        Y = y;
        Button = button;
        Delta = delta;
    }

    public MouseEvent(EventType type, int x, int y, MouseButtonFlags buttonFlags, MouseButton button = (MouseButton)(-1), bool isDoubleClick = false)
            : base(type)
    {
        X = x;
        Y = y;
        Button = button;
        ButtonFlags = buttonFlags;
        IsDoubleClick = isDoubleClick;
    }
}