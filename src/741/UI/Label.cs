using System.Drawing;
using DarkAges.Library.Graphics;
using Size = System.Drawing.Size;

namespace DarkAges.Library.UI;

public class Label : ControlPane
{
    public string Text { get; set; }
    public SimpleFont? Font { get; set; }
    public Color TextColor { get; set; }

    public Label()
    {
        Text = string.Empty;
        Font = FontManager.GetSimpleFont("default");
        TextColor = Color.Black;
    }

    public Label(string text, Point position)
    {
        Text = text;
        Bounds = new Rectangle(position, Size.Empty);
        Font = FontManager.GetSimpleFont("default");
        TextColor = Color.Black;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        if (Font != null && !string.IsNullOrEmpty(Text))
        {
            spriteBatch.DrawString(Font, Text, new Point(Bounds.X, Bounds.Y), TextColor);
        }
    }
}