using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class ButtonGroupViewPane : ControlPane
{
    private readonly List<TextButtonExControlPane> _buttons = [];
    private int _selectedIndex = -1;

    public event EventHandler<int> ButtonSelected;

    public void AddButton(string text)
    {
        var button = new TextButtonExControlPane(text);
        button.Position = new Point(50, 100 + _buttons.Count * 30);
        button.Click += (s, e) => SelectButton(_buttons.Count);
        _buttons.Add(button);
        AddChild(button);
    }

    private void SelectButton(int index)
    {
        _selectedIndex = index;
        ButtonSelected?.Invoke(this, index);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Button Group", 50, 50, Color.Black);
            
        base.Render(spriteBatch);
    }
}