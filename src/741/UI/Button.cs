using System;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using Size = System.Drawing.Size;

namespace DarkAges.Library.UI;

public class Button : ButtonControlPane
{
    public new string Text { get; set; }
    public SimpleFont Font { get; set; }
    public Color TextColor { get; set; }
    public Color BackColor { get; set; }
    public Color BorderColor { get; set; }

    public Button()
    {
        Text = string.Empty;
        Font = (FontManager.GetFont("default") as SimpleFont)!;
        TextColor = Color.White;
        BackColor = Color.Black;
        BorderColor = Color.White;
    }

    public Button(string text, Point position, Size size)
    {
        Text = text;
        Bounds = new Rectangle(position, size);
        Font = (FontManager.GetFont("default") as SimpleFont)!;
        TextColor = Color.White;
        BackColor = Color.Black;
        BorderColor = Color.White;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var color = IsEnabled ? BackColor : Color.Gray;
        if (State == 1) // Hover
            color = Color.DarkGray;
        if (State == 2) // Pressed
            color = Color.LightGray;

        spriteBatch.FillRectangle(Bounds, color);
        spriteBatch.DrawRectangle(Bounds, BorderColor);

        if (Font != null && !string.IsNullOrEmpty(Text))
        {
            var textSize = Font.MeasureString(Text);
            var textPosition = new Point(
                Bounds.X + (Bounds.Width - textSize.Width) / 2,
                Bounds.Y + (Bounds.Height - textSize.Height) / 2
            );
            spriteBatch.DrawString(Font, Text, textPosition, TextColor);
        }
    }
}