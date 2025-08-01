using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.MiniGame;

public class GameMenu : ControlPane
{
    private readonly List<TextButtonExControlPane> _gameButtons = [];
    private readonly List<string> _gameNames = ["Puzzle", "Rope Skipping", "Find Farmpet"];

    public event EventHandler<int> GameSelected;

    public GameMenu()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        for (var i = 0; i < _gameNames.Count; i++)
        {
            var button = new TextButtonExControlPane(_gameNames[i]);
            button.Position = new Point(200, 150 + i * 40);
            button.Click += (s, e) => GameSelected?.Invoke(this, i);
            _gameButtons.Add(button);
            AddChild(button);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Mini Games", 180, 100, Color.Black);
            
        base.Render(spriteBatch);
    }
}