using System.Drawing;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class ProgressBar : ControlPane
{
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    public int Value { get; set; }
    public Color BarColor { get; set; }
    public Color BackColor { get; set; }
    public Color BorderColor { get; set; }

    public ProgressBar(Rectangle bounds)
    {
        Bounds = bounds;
        Minimum = 0;
        Maximum = 100;
        Value = 0;
        LoadLayout();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_progressbar.txt");
            BarColor = layout.GetColor("BarColor", Color.Green);
            BackColor = layout.GetColor("BackColor", Color.Gray);
            BorderColor = layout.GetColor("BorderColor", Color.Black);
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error loading progress bar layout: {ex.Message}");
            BarColor = Color.Green;
            BackColor = Color.Gray;
            BorderColor = Color.Black;
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        spriteBatch.FillRectangle(Bounds, BackColor);

        var percent = (float)(Value - Minimum) / (Maximum - Minimum);
        var width = (int)(Bounds.Width * percent);
        var barRect = new Rectangle(Bounds.X, Bounds.Y, width, Bounds.Height);
        spriteBatch.FillRectangle(barRect, BarColor);

        spriteBatch.DrawRectangle(Bounds, BorderColor);
    }
}