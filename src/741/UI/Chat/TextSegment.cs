using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Chat;

public class TextSegment
{
    public string Text { get; set; }
    public Color Color { get; set; }
    public SimpleFont Font { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
}