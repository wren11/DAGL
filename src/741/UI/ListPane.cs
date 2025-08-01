using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI;

public class ListPane : ControlPane
{
    private readonly List<string> _items = [];
    private readonly SimpleFont _font;
    private int _selectedIndex = -1;
    private int _scrollOffset = 0;
    private readonly int _itemHeight = 20;
    private readonly int _maxVisibleItems = 10;

    public string Title { get; set; } = "";
    public bool ShowTitle { get; set; } = true;
    public bool AllowSelection { get; set; } = true;

    public event EventHandler<int> OnItemSelected = delegate { };

    public ListPane(SimpleFont font)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
    }

    public void AddItem(string item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public void RemoveItem(string item)
    {
        _items.Remove(item);
        if (_selectedIndex >= _items.Count)
        {
            _selectedIndex = _items.Count - 1;
        }
    }

    public void ClearItems()
    {
        _items.Clear();
        _selectedIndex = -1;
        _scrollOffset = 0;
    }

    public void Clear()
    {
        ClearItems();
    }

    public int ItemCount => _items.Count;

    public string? GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            return null;

        return _items[index];
    }

    public void SetItem(int index, string value)
    {
        if (index < 0 || index >= _items.Count)
            return;

        _items[index] = value;
    }

    public void Set(string[] items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        _items.Clear();
        _items.AddRange(items);
        _selectedIndex = -1;
        _scrollOffset = 0;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null) return;

        var startY = Bounds.Y;

        // Draw title
        if (ShowTitle && !string.IsNullOrEmpty(Title))
        {
            spriteBatch.DrawString(_font, Title, new DarkAges.Library.Graphics.Vector2(Bounds.X, startY), Color.Yellow);
            startY += _itemHeight;
        }

        // Draw items
        var visibleItems = Math.Min(_maxVisibleItems, _items.Count - _scrollOffset);
        for (var i = 0; i < visibleItems; i++)
        {
            var itemIndex = i + _scrollOffset;
            var itemText = _items[itemIndex];
            var itemY = startY + i * _itemHeight;
            var textColor = itemIndex == _selectedIndex ? Color.Yellow : Color.White;

            spriteBatch.DrawString(_font, itemText, new DarkAges.Library.Graphics.Vector2(Bounds.X, itemY), textColor);
        }

        // Draw scroll indicators if needed
        if (_items.Count > _maxVisibleItems)
        {
            if (_scrollOffset > 0)
            {
                spriteBatch.DrawString(_font, "▲", new DarkAges.Library.Graphics.Vector2(Bounds.Right - 20, Bounds.Y), Color.White);
            }
            if (_scrollOffset + _maxVisibleItems < _items.Count)
            {
                spriteBatch.DrawString(_font, "▼", new DarkAges.Library.Graphics.Vector2(Bounds.Right - 20, Bounds.Bottom - _itemHeight), Color.White);
            }
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !AllowSelection) return false;

        if (e is MouseEvent mouseEvent)
        {
            if (mouseEvent.Type == EventType.MouseDown && mouseEvent.Button == MouseButton.Left)
            {
                var startY = Bounds.Y + (ShowTitle && !string.IsNullOrEmpty(Title) ? _itemHeight : 0);
                var relativeY = mouseEvent.Y - startY;
                var clickedIndex = relativeY / _itemHeight + _scrollOffset;

                if (clickedIndex >= 0 && clickedIndex < _items.Count)
                {
                    SelectItem(clickedIndex);
                    return true;
                }
            }
        }
        else if (e is KeyEvent keyEvent && keyEvent.Type == EventType.KeyDown)
        {
            switch (keyEvent.Key)
            {
            case Silk.NET.Input.Key.Up:
                if (_selectedIndex > 0)
                {
                    SelectItem(_selectedIndex - 1);
                    EnsureItemVisible(_selectedIndex);
                    return true;
                }
                break;
            case Silk.NET.Input.Key.Down:
                if (_selectedIndex < _items.Count - 1)
                {
                    SelectItem(_selectedIndex + 1);
                    EnsureItemVisible(_selectedIndex);
                    return true;
                }
                break;
            case Silk.NET.Input.Key.PageUp:
                if (_selectedIndex > 0)
                {
                    var newIndex = Math.Max(0, _selectedIndex - _maxVisibleItems);
                    SelectItem(newIndex);
                    EnsureItemVisible(newIndex);
                    return true;
                }
                break;
            case Silk.NET.Input.Key.PageDown:
                if (_selectedIndex < _items.Count - 1)
                {
                    var newIndex = Math.Min(_items.Count - 1, _selectedIndex + _maxVisibleItems);
                    SelectItem(newIndex);
                    EnsureItemVisible(newIndex);
                    return true;
                }
                break;
            }
        }

        return false;
    }

    private void SelectItem(int index)
    {
        if (index != _selectedIndex)
        {
            _selectedIndex = index;
            OnItemSelected?.Invoke(this, index);
        }
    }

    private void EnsureItemVisible(int index)
    {
        if (index < _scrollOffset)
        {
            _scrollOffset = index;
        }
        else if (index >= _scrollOffset + _maxVisibleItems)
        {
            _scrollOffset = index - _maxVisibleItems + 1;
        }
    }

    public int GetSelectedIndex()
    {
        return _selectedIndex;
    }

    public string? GetSelectedItem()
    {
        return _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
    }
}