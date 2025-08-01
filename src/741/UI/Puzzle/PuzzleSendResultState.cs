using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleSendResultState(PuzzleGame game) : PuzzleGameState(game)
{
    private PuzzleSendResultPane _sendResultPane = new();

    public override void Initialize()
    {
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _sendResultPane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        return _sendResultPane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Dispose()
    {
        _sendResultPane?.Dispose();
    }
}