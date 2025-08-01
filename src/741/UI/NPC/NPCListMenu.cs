using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public class NPCListMenu : NPCMenu
{
    private readonly List<string> _listItems = [];
    private int _listSelectedIndex = -1;

    public event EventHandler<ListItemEventArgs> ListItemSelected;

    public void AddListItem(string item)
    {
        if (!string.IsNullOrEmpty(item))
        {
            _listItems.Add(item);
        }
    }

    public void RemoveListItem(string item)
    {
        _listItems.Remove(item);
    }

    public void ClearListItems()
    {
        _listItems.Clear();
        _listSelectedIndex = -1;
    }

    public void SelectListItem(int index)
    {
        if (index >= 0 && index < _listItems.Count)
        {
            _listSelectedIndex = index;
            ListItemSelected?.Invoke(this, new ListItemEventArgs(_listItems[index], index));
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || !_isVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        spriteBatch.FillRectangle(_menuBounds, _backgroundColor);
        spriteBatch.DrawRectangle(_menuBounds, _borderColor);

        var itemHeight = 25;
        var startY = _menuBounds.Y + 10;

        for (var i = 0; i < _listItems.Count; i++)
        {
            var item = _listItems[i];
            var itemRect = new Rectangle(_menuBounds.X + 5, startY + i * itemHeight, _menuBounds.Width - 10, itemHeight - 2);

            if (i == _listSelectedIndex)
            {
                spriteBatch.FillRectangle(itemRect, _selectedColor);
            }

            var textColor = i == _listSelectedIndex ? System.Drawing.Color.Yellow : _textColor;
            //spriteBatch.DrawString(font, item, itemRect.X + 5, itemRect.Y + 5, textColor);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !_isVisible) return false;

        if (e is KeyEvent keyEvent)
        {
            if (keyEvent.Type == EventType.KeyDown)
            {
                switch (keyEvent.Key)
                {
                case Silk.NET.Input.Key.Up:
                    if (_listSelectedIndex > 0)
                        SelectListItem(_listSelectedIndex - 1);
                    return true;
                case Silk.NET.Input.Key.Down:
                    if (_listSelectedIndex < _listItems.Count - 1)
                        SelectListItem(_listSelectedIndex + 1);
                    return true;
                case Silk.NET.Input.Key.Enter:
                    if (_listSelectedIndex >= 0)
                    {
                        var selectedItem = _menuItems.Find(item => item.Text == _listItems[_listSelectedIndex]);
                        if (selectedItem != null)
                        {
                            selectedItem.Execute();
                        }
                    }
                    return true;
                case Silk.NET.Input.Key.Escape:
                    Hide();
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }
}