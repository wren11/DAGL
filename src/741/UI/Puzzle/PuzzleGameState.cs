using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public abstract class PuzzleGameState(PuzzleGame game)
{
    protected readonly PuzzleGame _game = game;

    public abstract void Initialize();
    public abstract void Render(SpriteBatch spriteBatch);
    public abstract bool HandleEvent(Event e);
    public abstract void Update(float deltaTime);
    public abstract void Dispose();
}