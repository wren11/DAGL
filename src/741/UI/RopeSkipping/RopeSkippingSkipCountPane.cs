using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingSkipCountPane : ControlPane
{
    private int _skipCount = 0;
    private TextEditControlPane _countLabel;

    public RopeSkippingSkipCountPane()
    {
        _countLabel = new TextEditControlPane("Skips: 0", new Rectangle(400, 110, 150, 25), true);
        AddChild(_countLabel);
    }

    public void UpdateCount(int count)
    {
        _skipCount = count;
        //_countLabel.Text = $"Skips: {_skipCount}";
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Skip Counter", 400, 90, Color.Black);
            
        base.Render(spriteBatch);
    }
}