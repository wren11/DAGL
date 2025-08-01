namespace DarkAges.Library.Core.Events;

public class KeyPressEventArgs : EventArgs
{
    public char KeyChar { get; }
    public bool Handled { get; set; }

    public KeyPressEventArgs(char keyChar)
    {
        KeyChar = keyChar;
    }
}