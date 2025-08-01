using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class TabButtonPane : ControlPane
{
    private readonly List<TextButtonExControlPane> _tabButtons = [];
    public int ActiveTab { get; private set; } = 0;

    public event EventHandler<int> TabChanged;

    public void AddTab(string tabName)
    {
        var button = new TextButtonExControlPane(tabName);
        button.Position = new Point(50 + _tabButtons.Count * 100, 50);
        button.Click += (s, e) => SelectTab(_tabButtons.Count);
        _tabButtons.Add(button);
        AddChild(button);
    }

    private void SelectTab(int index)
    {
        ActiveTab = index;
        TabChanged?.Invoke(this, index);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Tab Navigation", 50, 20, Color.Black);
            
        base.Render(spriteBatch);
    }
}