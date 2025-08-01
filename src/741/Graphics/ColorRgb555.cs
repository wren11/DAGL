namespace DarkAges.Library.Graphics;

/// <summary>
/// Represents a 15-bit color in RGB 5-5-5 format.
/// </summary>
public struct ColorRgb555
{
    public ushort Value { get; set; }

    public byte R
    {
        get => (byte)((Value & 0x7C00) >> 10);
        set => Value = (ushort)((Value & 0x83FF) | (value << 10));
    }

    public byte G
    {
        get => (byte)((Value & 0x03E0) >> 5);
        set => Value = (ushort)((Value & 0xFC1F) | (value << 5));
    }

    public byte B
    {
        get => (byte)(Value & 0x001F);
        set => Value = (ushort)((Value & 0xFFE0) | value);
    }

    public ColorRgb555(ushort value)
    {
        Value = value;
    }

    public ColorRgb555(byte r, byte g, byte b)
    {
        Value = (ushort)((r << 10) | (g << 5) | b);
    }
}