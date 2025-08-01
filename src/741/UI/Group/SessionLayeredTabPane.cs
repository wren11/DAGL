using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class SessionLayeredTabPane : TabButtonPane
{
    private readonly List<ControlPane> _layers = [];

    public void AddLayer(ControlPane layer)
    {
        _layers.Add(layer);
        AddChild(layer);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        base.Render(spriteBatch);

        if (ActiveTab >= 0 && ActiveTab < _layers.Count)
        {
            _layers[ActiveTab].Render(spriteBatch);
        }
    }
}