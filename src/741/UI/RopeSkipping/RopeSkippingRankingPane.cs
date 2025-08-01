using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingRankingPane : ControlPane
{
    private TextButtonExControlPane _backButton;

    public RopeSkippingRankingPane()
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

        //spriteBatch.DrawString(font, "Ranking", 200, 100, Color.Black);
            
        base.Render(spriteBatch);
    }
}