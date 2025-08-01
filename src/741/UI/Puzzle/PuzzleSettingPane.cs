using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleSettingPane : ControlPane
{
    private TextButtonExControlPane _backButton;

    public PuzzleSettingPane()
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

        //spriteBatch.DrawString(font, "Settings", 200, 100, Color.Black);
            
        base.Render(spriteBatch);
    }
}