using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingHelpPane : ControlPane
{
    private TextButtonExControlPane _backButton;

    public RopeSkippingHelpPane()
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
        //spriteBatch.DrawString(font, "Press SPACEBAR to jump over the ropes", 100, 130, Color.Black);
            
        base.Render(spriteBatch);
    }
}