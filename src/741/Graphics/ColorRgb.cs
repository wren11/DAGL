using System.Drawing;

namespace DarkAges.Library.Graphics;

public struct ColorRgb(byte r, byte g, byte b)
{
    public byte R { get; set; } = r;
    public byte G { get; set; } = g;
    public byte B { get; set; } = b;

    public static implicit operator Color(ColorRgb rgb)
    {
        return Color.FromArgb(rgb.R, rgb.G, rgb.B);
    }

    public static implicit operator ColorRgb(Color color)
    {
        return new ColorRgb(color.R, color.G, color.B);
    }
}