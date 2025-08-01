using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleResultState(PuzzleGame game) : PuzzleGameState(game)
{
    private PuzzleResultPane _resultPane = new();
    private int _finalScore;

    public override void Initialize()
    {
        _finalScore = 0;
        _resultPane.SetScore(_finalScore);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _resultPane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        return _resultPane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Dispose()
    {
        _resultPane?.Dispose();
    }
}