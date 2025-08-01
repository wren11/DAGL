using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class KeyboardInfoPane : InfoPane
{
    private readonly Dictionary<string, string> _keyBindings = new Dictionary<string, string>
    {
        {"Move", "WASD / Arrow Keys"},
        {"Attack", "Space"},
        {"Inventory", "I"},
        {"Character", "C"},
        {"Map", "M"},
        {"Chat", "Enter"},
        {"Escape", "ESC"}
    };

    public KeyboardInfoPane()
    {
        _title = "Keyboard Controls";
    }

    protected override void RenderContent(SpriteBatch spriteBatch)
    {
        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Key Bindings:", 70, 100, Color.Black);

        var y = 120;
        foreach (var binding in _keyBindings)
        {
            //spriteBatch.DrawString(font, $"{binding.Key}: {binding.Value}", 70, y, Color.Black);
            y += 20;
        }
    }
}