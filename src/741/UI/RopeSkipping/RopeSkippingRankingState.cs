using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingRankingState(RopeSkippingGame game) : RopeSkippingGameState(game)
{
    private RopeSkippingRankingPane _rankingPane = new();

    public override void Initialize()
    {
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _rankingPane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        return _rankingPane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Dispose()
    {
        _rankingPane?.Dispose();
    }
}