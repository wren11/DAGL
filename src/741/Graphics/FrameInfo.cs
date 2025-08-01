using System.Drawing;

namespace DarkAges.Library.Graphics;

public class FrameInfo(IndexedImage image, Rectangle sourceRect, float duration = 0.1f)
{
    public IndexedImage Image { get; set; } = image;
    public Rectangle SourceRect { get; set; } = sourceRect;
    public float Duration { get; set; } = duration;
    public int DataOffset { get; set; }
    public int X1 => SourceRect.X;
    public int Y1 => SourceRect.Y;
    public int X2 => SourceRect.X + SourceRect.Width;
    public int Y2 => SourceRect.Y + SourceRect.Height;
}