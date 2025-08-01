using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingGameControlPane : ControlPane
{
    private int _score = 0;
    private int _timeRemaining = 60;
    private TextEditControlPane _scoreLabel;
    private TextEditControlPane _timeLabel;

    public RopeSkippingGameControlPane()
    {
        _scoreLabel = new TextEditControlPane("Score: 0", new Rectangle(400, 50, 150, 25), true);
        _timeLabel = new TextEditControlPane("Time: 60", new Rectangle(400, 80, 150, 25), true);

        AddChild(_scoreLabel);
        AddChild(_timeLabel);
    }

    public void UpdateScore(int score)
    {
        _score = score;
        //_scoreLabel.Text = $"Score: {_score}";
    }

    public void UpdateTime(int time)
    {
        _timeRemaining = time;
        //_timeLabel.Text = $"Time: {_timeRemaining}";
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Game Info", 400, 20, Color.Black);
            
        base.Render(spriteBatch);
    }
}