using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingResultPane : ControlPane
{
    private int _score = 0;
    private int _skipCount = 0;
    private TextEditControlPane _scoreLabel;
    private TextEditControlPane _skipCountLabel;
    private TextButtonExControlPane _retryButton;
    private TextButtonExControlPane _exitButton;

    public RopeSkippingResultPane()
    {
        _scoreLabel = new TextEditControlPane("Final Score: 0", new Rectangle(200, 150, 200, 30), true);
        _skipCountLabel = new TextEditControlPane("Total Skips: 0", new Rectangle(200, 180, 200, 30), true);
        _retryButton = new TextButtonExControlPane("Retry");
        _exitButton = new TextButtonExControlPane("Exit");

        _retryButton.Position = new Point(200, 220);
        _exitButton.Position = new Point(200, 250);

        AddChild(_scoreLabel);
        AddChild(_skipCountLabel);
        AddChild(_retryButton);
        AddChild(_exitButton);
    }

    public void SetScore(int score)
    {
        _score = score;
        //_scoreLabel.Text = $"Final Score: {_score}";
    }

    public void SetSkipCount(int skipCount)
    {
        _skipCount = skipCount;
        //_skipCountLabel.Text = $"Total Skips: {_skipCount}";
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Game Over", 200, 100, Color.Black);
            
        base.Render(spriteBatch);
    }
}