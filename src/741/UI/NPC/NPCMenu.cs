using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public abstract class NPCMenu : ControlPane
{
    protected readonly List<NPCMenuItem> _menuItems = [];
    protected NPCMenuItem _selectedItem;
    protected int _selectedIndex = -1;
    protected bool _isVisible = false;
    protected Rectangle _menuBounds = new(100, 100, 300, 200);
    protected System.Drawing.Color _backgroundColor = System.Drawing.Color.FromArgb(200, 0, 0, 0);
    protected System.Drawing.Color _borderColor = System.Drawing.Color.White;
    protected System.Drawing.Color _selectedColor = System.Drawing.Color.Blue;
    protected System.Drawing.Color _textColor = System.Drawing.Color.White;

    public event EventHandler<NPCMenuItemEventArgs> ItemSelected;
    public event EventHandler<NPCMenuItemEventArgs> ItemClicked;

    public virtual void AddMenuItem(NPCMenuItem item)
    {
        if (item != null)
        {
            _menuItems.Add(item);
        }
    }

    public virtual void RemoveMenuItem(NPCMenuItem item)
    {
        _menuItems.Remove(item);
    }

    public virtual void ClearMenuItems()
    {
        _menuItems.Clear();
        _selectedIndex = -1;
        _selectedItem = null;
    }

    public virtual void SelectItem(int index)
    {
        if (index >= 0 && index < _menuItems.Count)
        {
            _selectedIndex = index;
            _selectedItem = _menuItems[index];
            ItemSelected?.Invoke(this, new NPCMenuItemEventArgs(_selectedItem));
        }
    }

    public virtual void SelectNextItem()
    {
        if (_menuItems.Count == 0) return;

        _selectedIndex = (_selectedIndex + 1) % _menuItems.Count;
        _selectedItem = _menuItems[_selectedIndex];
        ItemSelected?.Invoke(this, new NPCMenuItemEventArgs(_selectedItem));
    }

    public virtual void SelectPreviousItem()
    {
        if (_menuItems.Count == 0) return;

        _selectedIndex = (_selectedIndex - 1 + _menuItems.Count) % _menuItems.Count;
        _selectedItem = _menuItems[_selectedIndex];
        ItemSelected?.Invoke(this, new NPCMenuItemEventArgs(_selectedItem));
    }

    public virtual void ExecuteSelectedItem()
    {
        if (_selectedItem != null)
        {
            _selectedItem.Execute();
            ItemClicked?.Invoke(this, new NPCMenuItemEventArgs(_selectedItem));
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

        for (var i = 0; i < _menuItems.Count; i++)
        {
            var item = _menuItems[i];
            var itemRect = new Rectangle(_menuBounds.X + 5, startY + i * itemHeight, _menuBounds.Width - 10, itemHeight - 2);

            if (i == _selectedIndex)
            {
                spriteBatch.FillRectangle(itemRect, _selectedColor);
            }

            var textColor = i == _selectedIndex ? System.Drawing.Color.Yellow : _textColor;
            //spriteBatch.DrawString(font, item.Text, itemRect.X + 5, itemRect.Y + 5, textColor);
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
                    SelectPreviousItem();
                    return true;
                case Silk.NET.Input.Key.Down:
                    SelectNextItem();
                    return true;
                case Silk.NET.Input.Key.Enter:
                    ExecuteSelectedItem();
                    return true;
                case Silk.NET.Input.Key.Escape:
                    Hide();
                    return true;
                }
            }
        }

        if (e is MouseEvent mouseEvent)
        {
            if (mouseEvent.Button == Core.Events.MouseButton.Left && mouseEvent.Type == EventType.MouseDown)
            {
                var itemHeight = 25;
                var startY = _menuBounds.Y + 10;
                var relativeY = mouseEvent.Y - startY;
                var clickedIndex = relativeY / itemHeight;

                if (clickedIndex >= 0 && clickedIndex < _menuItems.Count)
                {
                    SelectItem(clickedIndex);
                    ExecuteSelectedItem();
                    return true;
                }
            }
        }

        return false;
    }

    public virtual void Show()
    {
        _isVisible = true;
        if (_menuItems.Count > 0 && _selectedIndex == -1)
        {
            SelectItem(0);
        }
    }

    public virtual void Hide()
    {
        _isVisible = false;
        _selectedIndex = -1;
        _selectedItem = null;
    }

    public new void SetBounds(Rectangle bounds)
    {
        _menuBounds = bounds;
    }

    public List<NPCMenuItem> GetMenuItems()
    {
        return [.._menuItems];
    }

    public NPCMenuItem GetSelectedItem()
    {
        return _selectedItem;
    }

    public int GetSelectedIndex()
    {
        return _selectedIndex;
    }
}