using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleHelpPane : ControlPane
{
    private TextButtonExControlPane _backButton;

    public PuzzleHelpPane()
    {
        _backButton = new TextButtonExControlPane("Back");
        _backButton.Position = new Point(200, 200);
        AddChild(_backButton);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Help", 200, 100, Color.Black);
        //spriteBatch.DrawString(font, "Click adjacent tiles of the same color to remove them", 100, 130, Color.Black);
            
        base.Render(spriteBatch);
    }
}