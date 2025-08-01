
namespace DarkAges.Library.Graphics;

public class Size(int width, int height)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public int X { get; set; }
    public int Y { get; set; }

    public static Size Empty => new Size(0, 0);

    public static implicit operator System.Drawing.Size(Size v)
    {
        throw new NotImplementedException();
    }
}