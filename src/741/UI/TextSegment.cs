using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class TextSegment(string text, ColorRgb565 color)
{
    public string Text { get; set; } = text;
    public ColorRgb565 Color { get; set; } = color;
}