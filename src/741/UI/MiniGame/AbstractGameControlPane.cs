using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.MiniGame;

public class AbstractGameControlPane : ControlPane
{
    protected TextButtonExControlPane _startButton = null!;
    protected TextButtonExControlPane _pauseButton = null!;
    protected TextButtonExControlPane _exitButton = null!;
    protected TextEditControlPane _scoreLabel = null!;
    protected TextEditControlPane _levelLabel = null!;

    public event EventHandler StartClicked = null!;
    public event EventHandler PauseClicked = null!;
    public event EventHandler ExitClicked = null!;

    public AbstractGameControlPane()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        _startButton = new TextButtonExControlPane("Start");
        _pauseButton = new TextButtonExControlPane("Pause");
        _exitButton = new TextButtonExControlPane("Exit");
        _scoreLabel = new TextEditControlPane("Score: 0", new Rectangle(400, 50, 150, 25), true);
        _levelLabel = new TextEditControlPane("Level: 1", new Rectangle(400, 80, 150, 25), true);

        _startButton.Position = new Point(400, 120);
        _pauseButton.Position = new Point(400, 150);
        _exitButton.Position = new Point(400, 180);

        _startButton.Click += (s, e) => StartClicked?.Invoke(this, e);
        _pauseButton.Click += (s, e) => PauseClicked?.Invoke(this, e);
        _exitButton.Click += (s, e) => ExitClicked?.Invoke(this, e);

        AddChild(_startButton);
        AddChild(_pauseButton);
        AddChild(_exitButton);
        AddChild(_scoreLabel);
        AddChild(_levelLabel);
    }

    public void UpdateScore(int score)
    {
        _scoreLabel.Text = $"Score: {score}";
    }

    public void UpdateLevel(int level)
    {
        _levelLabel.Text = $"Level: {level}";
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Game Controls", 400, 20, Color.Black);
            
        base.Render(spriteBatch);
    }
}