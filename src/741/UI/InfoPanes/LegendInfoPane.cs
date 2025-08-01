using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class LegendInfoPane : InfoPane
{
    private readonly Dictionary<string, string> _legendItems = new Dictionary<string, string>
    {
        {"Town", "Safe zone with services"},
        {"Dungeon", "Dangerous area with monsters"},
        {"Shop", "Buy and sell items"},
        {"Inn", "Rest and recover health"},
        {"Quest", "Available quest location"},
        {"Portal", "Travel to another area"}
    };

    public LegendInfoPane()
    {
        _title = "Map Legend";
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Map Symbols:", 70, 100, Color.Black);

        var y = 120;
        foreach (var item in _legendItems)
        {
            //spriteBatch.DrawString(font, $"{item.Key}: {item.Value}", 70, y, Color.Black);
            y += 20;
        }
    }
}