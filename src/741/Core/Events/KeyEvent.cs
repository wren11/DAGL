using Silk.NET.Input;

namespace DarkAges.Library.Core.Events;

public class KeyEvent(EventType type, Key key, bool isRepeat = false, KeyModifiers modifiers = KeyModifiers.None)
        : Event(type)
{
    public Key Key { get; set; } = key;
    public bool IsRepeat { get; set; } = isRepeat;
    public KeyModifiers Modifiers { get; set; } = modifiers;
    public bool IsKeyDown { get; set; }
    public int KeyCode { get; internal set; }
}