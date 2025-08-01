using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.GameLogic;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class ExchangeItemListPane : ControlPane
{
    private readonly List<Item> _items = [];
    private int _selectedIndex = -1;
    private int _scrollOffset = 0;
    private SimpleFont? _font;
    private GraphicsDevice _graphicsDevice;
    private int _itemHeight;
    private int _iconSize;
    private Color _backgroundColor;
    private Color _borderColor;
    private Color _textColor;
    private Color _selectedColor;
    private Color _selectedTextColor;
    private Color _valueColor;

    public event EventHandler<Item> ItemSelected;

    public ExchangeItemListPane(Rectangle rect)
    {
        Bounds = rect;
        LoadLayout();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_exchangeitemlist.txt");
            _itemHeight = layout.GetInt("ItemHeight", 30);
            _iconSize = layout.GetInt("IconSize", 24);
            _backgroundColor = layout.GetColor("BackgroundColor", Color.White);
            _borderColor = layout.GetColor("BorderColor", Color.Black);
            _textColor = layout.GetColor("TextColor", Color.Black);
            _selectedColor = layout.GetColor("SelectedColor", Color.LightBlue);
            _selectedTextColor = layout.GetColor("SelectedTextColor", Color.Blue);
            _valueColor = layout.GetColor("ValueColor", Color.Gray);
            var fontName = layout.GetString("FontName", "default");
            _font = FontManager.GetFont(fontName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading exchange item list layout: {ex.Message}");
            _itemHeight = 30;
            _iconSize = 24;
            _backgroundColor = Color.White;
            _borderColor = Color.Black;
            _textColor = Color.Black;
            _selectedColor = Color.LightBlue;
            _selectedTextColor = Color.Blue;
            _valueColor = Color.Gray;
            _font = FontManager.GetFont("default");
        }
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        var index = _items.IndexOf(item);
        if (index >= 0)
        {
            _items.RemoveAt(index);
            if (_selectedIndex == index)
            {
                _selectedIndex = -1;
            }
            else if (_selectedIndex > index)
            {
                _selectedIndex--;
            }
        }
    }

    public void ClearItems()
    {
        _items.Clear();
        _selectedIndex = -1;
        _scrollOffset = 0;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Render background
        spriteBatch.DrawRectangle(Bounds, _backgroundColor);
        spriteBatch.DrawRectangle(Bounds, _borderColor);

        // Calculate visible items
        var visibleItems = Bounds.Height / _itemHeight;
        var startIndex = _scrollOffset;
        var endIndex = Math.Min(startIndex + visibleItems, _items.Count);

        // Render visible items
        for (var i = startIndex; i < endIndex; i++)
        {
            var y = Bounds.Y + (i - startIndex) * _itemHeight;
            var item = _items[i];
                
            // Highlight selected item
            if (i == _selectedIndex)
            {
                spriteBatch.FillRectangle(
                    new Rectangle(Bounds.X, y, Bounds.Width, _itemHeight),
                    _selectedColor
                );
            }

            // Render item icon
            if (item.Icon != null)
            {
                spriteBatch.DrawImage(
                    item.Icon,
                    Bounds.X + 2,
                    y + (_itemHeight - _iconSize) / 2,
                    _iconSize,
                    _iconSize
                );
            }

            // Render item name
            var displayText = $"{item.Name} x{item.Quantity}";
            spriteBatch.DrawString(
                _font,
                displayText,
                Bounds.X + _iconSize + 8,
                y + 5,
                i == _selectedIndex ? _selectedTextColor : _textColor
            );

            // Render item value if available
            if (item.Value > 0)
            {
                var valueText = $"{item.Value:N0} gold";
                var valueSize = _font.MeasureString(valueText);
                spriteBatch.DrawString(
                    _font,
                    valueText,
                    Bounds.X + Bounds.Width - valueSize.Width - 5,
                    y + 5,
                    _valueColor
                );
            }
        }

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Core.Events.Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        if (e is MouseEvent me && Bounds.Contains(me.X, me.Y))
        {
            if (me.Type == EventType.LButtonDown)
            {
                // Calculate which item was clicked
                var relativeY = me.Y - Bounds.Y;
                var clickedIndex = _scrollOffset + (relativeY / _itemHeight);
                    
                if (clickedIndex >= 0 && clickedIndex < _items.Count)
                {
                    _selectedIndex = clickedIndex;
                    ItemSelected?.Invoke(this, _items[clickedIndex]);
                    return true;
                }
            }
            else if (me.Type == EventType.MouseWheel)
            {
                // Handle scrolling
                if (me.Delta > 0 && _scrollOffset > 0)
                {
                    _scrollOffset--;
                }
                else if (me.Delta < 0 && _scrollOffset + (Bounds.Height / _itemHeight) < _items.Count)
                {
                    _scrollOffset++;
                }
                return true;
            }
        }

        return base.HandleEvent(e);
    }

    public Item GetSelectedItem()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
            return _items[_selectedIndex];
        return null;
    }

    public List<Item> GetItems()
    {
        return [.._items];
    }

    public void SetSelectedItem(Item item)
    {
        var index = _items.IndexOf(item);
        _selectedIndex = index;
    }
}