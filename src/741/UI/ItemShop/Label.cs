using System.Drawing;

namespace DarkAges.Library.UI.ItemShop;

public class Label : ControlPane
{
    public string Text { get; set; }

    public Label(string text, Point position)
    {
        Text = text;
        Position = position;
    }

    public override void Render(Graphics.SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;
        var font = Graphics.FontManager.GetSimpleFont("default");
        if (font != null)
        {
            spriteBatch.DrawString(font, Text, Position, System.Drawing.Color.White);
        }
    }
}