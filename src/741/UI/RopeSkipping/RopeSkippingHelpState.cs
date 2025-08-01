using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingHelpState(RopeSkippingGame game) : RopeSkippingGameState(game)
{
    private RopeSkippingHelpPane _helpPane = new();

    public override void Initialize()
    {
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _helpPane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        return _helpPane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Dispose()
    {
        _helpPane?.Dispose();
    }
}