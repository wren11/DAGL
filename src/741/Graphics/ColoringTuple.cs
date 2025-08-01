namespace DarkAges.Library.Graphics;

public class ColoringTuple
{
    public short[] Colors { get; }
    public int OriginalColor { get; set; }
    public int Index => OriginalColor;
    public ColorRgb Color => new((byte)Colors[0], (byte)Colors[1], (byte)Colors[2]);

    public ColoringTuple(int originalColor, short[] colors)
    {
        OriginalColor = originalColor;
        Colors = colors;
    }
}