using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleTitleState(PuzzleGame game) : PuzzleGameState(game)
{
    private PuzzleTitlePane _titlePane = new();

    public override void Initialize()
    {
        _titlePane.StartButton.Click += (s, e) => _game.SetState(1);
        _titlePane.SettingButton.Click += (s, e) => _game.SetState(4);
        _titlePane.RankingButton.Click += (s, e) => _game.SetState(5);
        _titlePane.HelpButton.Click += (s, e) => _game.SetState(6);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _titlePane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        return _titlePane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Dispose()
    {
        _titlePane?.Dispose();
    }
}