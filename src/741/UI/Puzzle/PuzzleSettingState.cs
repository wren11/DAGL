using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleSettingState(PuzzleGame game) : PuzzleGameState(game)
{
    private PuzzleSettingPane _settingPane = new();

    public override void Initialize()
    {
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _settingPane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        return _settingPane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Dispose()
    {
        _settingPane?.Dispose();
    }
}