using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleGameControlPane : ControlPane
{
    private int _score = 0;
    private int _level = 1;
    private int _timeRemaining = 300;
    private TextEditControlPane _scoreLabel;
    private TextEditControlPane _levelLabel;
    private TextEditControlPane _timeLabel;

    public PuzzleGameControlPane()
    {
        _scoreLabel = new TextEditControlPane("Score: 0", new Rectangle(400, 50, 150, 25), true);
        _levelLabel = new TextEditControlPane("Level: 1", new Rectangle(400, 80, 150, 25), true);
        _timeLabel = new TextEditControlPane("Time: 300", new Rectangle(400, 110, 150, 25), true);

        AddChild(_scoreLabel);
        AddChild(_levelLabel);
        AddChild(_timeLabel);
    }

    public void UpdateScore(int score)
    {
        _score = score;
        _scoreLabel.Text = $"Score: {_score}";
    }

    public void UpdateLevel(int level)
    {
        _level = level;
        _levelLabel.Text = $"Level: {_level}";
    }

    public void UpdateTime(int time)
    {
        _timeRemaining = time;
        _timeLabel.Text = $"Time: {_timeRemaining}";
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