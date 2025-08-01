using DarkAges.Library.Graphics;
using System.Drawing;
using Size = System.Drawing.Size;

namespace DarkAges.Library.UI;

public class DescPane : ControlPane
{
    private readonly TextFlowPane _textPane;

    public Point Position { get; internal set; }

    public DescPane()
    {
        _textPane = new TextFlowPane(new Rectangle(0, 0, 100, 100));
        AddChild(_textPane);

        IsVisible = false; 
    }

    public void Show(string text, Point position, System.Drawing.Size size)
    {
        _textPane.Text = text;
        Size = size;
        Position = position;
        _textPane.Position = new Point(6, 6);
        _textPane.Size = new Size(size.Width - 12, size.Height - 12);

        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // This simulates the 9-patch border drawing from sub_49CCF0
        // Since we don't have the assets, we'll draw a simple border.
        // A real implementation would draw 8 border segment textures and a tiled background.
        spriteBatch.DrawRectangle(new Rectangle(Bounds.X + 1, Bounds.Y + 1, Bounds.Width - 2, Bounds.Height - 2), ColorRgb565.Black);
        spriteBatch.DrawRectangle(new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height), ColorRgb565.White, false);
            
        base.Render(spriteBatch);
    }
}