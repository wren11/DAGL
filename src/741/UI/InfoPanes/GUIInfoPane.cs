using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class GUIInfoPane : InfoPane
{
    private readonly List<string> _guiElements =
    [
        "Health Bar - Shows your current health",
        "Mana Bar - Shows your current mana",
        "Experience Bar - Shows your progress to next level",
        "Mini Map - Shows your current location",
        "Chat Window - For communication with other players",
        "Inventory Panel - Manage your items",
        "Character Panel - View your stats and equipment"
    ];

    public GUIInfoPane()
    {
        _title = "GUI Information";
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Interface Elements:", 70, 100, Color.Black);

        for (var i = 0; i < _guiElements.Count; i++)
        {
            //spriteBatch.DrawString(font, $"{i + 1}. {_guiElements[i]}", 70, 120 + i * 20, Color.Black);
        }
    }
}