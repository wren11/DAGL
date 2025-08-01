using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class HelpInfoPane : InfoPane
{
    private readonly List<string> _helpTopics =
    [
        "Basic Controls",
        "Combat System",
        "Inventory Management",
        "Character Development",
        "Group System",
        "Trading"
    ];

    public HelpInfoPane()
    {
        _title = "Help Information";
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Help Topics:", 70, 100, Color.Black);

        for (var i = 0; i < _helpTopics.Count; i++)
        {
            //spriteBatch.DrawString(font, $"{i + 1}. {_helpTopics[i]}", 70, 120 + i * 20, Color.Black);
        }
    }
}