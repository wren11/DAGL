using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingResultState(RopeSkippingGame game) : RopeSkippingGameState(game)
{
    private RopeSkippingResultPane _resultPane = new();
    private int _finalScore;
    private int _finalSkipCount;

    public override void Initialize()
    {
        _finalScore = 0;
        _finalSkipCount = 0;
        _resultPane.SetScore(_finalScore);
        _resultPane.SetSkipCount(_finalSkipCount);
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