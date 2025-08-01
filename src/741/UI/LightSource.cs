using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class LightSource
{
    public Point Position { get; set; }
    public int Radius { get; set; }
    public ColorRgb Color { get; set; }
    public bool IsActive { get; set; }
}