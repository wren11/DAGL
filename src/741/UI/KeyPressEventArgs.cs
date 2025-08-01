using System;

namespace DarkAges.Library.UI;

public class KeyPressEventArgs : EventArgs
{
    public char KeyChar { get; }
    public bool Handled { get; set; }

    public KeyPressEventArgs(char keyChar)
    {
        KeyChar = keyChar;
        Handled = false;
    }
} 