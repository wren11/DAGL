using System.Drawing;

namespace DarkAges.Library.UI;

public class CreditEntry
{
    public CreditEntry()
    {
    }

    public CreditEntry(string text, CreditType type, Color color)
    {
        Text = text;
        Type = type;
        Color = color;
    }

    public CreditEntry(CreditType type, string text, Color color)
    {
        Type = type;
        Text = text;
        Color = color;
    }

    public string Text { get; init; } = null!;
    public CreditType Type { get; set; }
    public Color Color { get; set; }
}