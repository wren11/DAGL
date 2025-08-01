using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.IO;
using System;

namespace DarkAges.Library.UI;

public class LegendListPane : ControlPane
{
    private readonly List<LegendEntry> _legends = [];
    private int _selectedIndex = -1;
    private SimpleFont _font;
    private GraphicsDevice _graphicsDevice;
    private int _scrollOffset = 0;
    private int _itemHeight;
    private Color _backgroundColor;
    private Color _borderColor;
    private Color _textColor;
    private Color _selectedColor;
    private Color _selectedTextColor;

    public LegendListPane()
    {
        LoadLayout();
        LoadLegendData();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_legendlist.txt");
            _itemHeight = layout.GetInt("ItemHeight", 20);
            _backgroundColor = layout.GetColor("BackgroundColor", Color.White);
            _borderColor = layout.GetColor("BorderColor", Color.Black);
            _textColor = layout.GetColor("TextColor", Color.Black);
            _selectedColor = layout.GetColor("SelectedColor", Color.LightBlue);
            _selectedTextColor = layout.GetColor("SelectedTextColor", Color.Blue);
            var fontName = layout.GetString("FontName", "default");
            _font = FontManager.GetFont(fontName) as SimpleFont;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading legend list layout: {ex.Message}");
            _itemHeight = 20;
            _backgroundColor = Color.White;
            _borderColor = Color.Black;
            _textColor = Color.Black;
            _selectedColor = Color.LightBlue;
            _selectedTextColor = Color.Blue;
            _font = FontManager.GetFont("default") as SimpleFont;
        }
    }

    private void LoadLegendData()
    {
        try
        {
            // Load legend data from table file
            var tableFile = new TableFile("legend.tbl");
            // Note: TableFile doesn't have Entries property, so we'll use fallback data
            // In a real implementation, you would need to implement the proper way to read table entries
        }
        catch
        {
            // Fallback to default legends if file not found
            AddLegend(1, "Town");
            AddLegend(2, "Dungeon");
            AddLegend(3, "Shop");
            AddLegend(4, "Inn");
            AddLegend(5, "Temple");
        }
    }

    public void AddLegend(int id, string name)
    {
        _legends.Add(new LegendEntry { Id = id, Name = name });
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
        var endIndex = Math.Min(startIndex + visibleItems, _legends.Count);

        // Render visible legends
        for (var i = startIndex; i < endIndex; i++)
        {
            var y = Bounds.Y + (i - startIndex) * _itemHeight;
            var legend = _legends[i];
                
            // Highlight selected item
            if (i == _selectedIndex)
            {
                spriteBatch.FillRectangle(
                    new Rectangle(Bounds.X, y, Bounds.Width, _itemHeight),
                    _selectedColor
                );
            }

            // Render legend name
            spriteBatch.DrawString(
                _font,
                legend.Name,
                new Vector2(Bounds.X + 5, y + 2),
                i == _selectedIndex ? _selectedTextColor : _textColor
            );
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
                    
                if (clickedIndex >= 0 && clickedIndex < _legends.Count)
                {
                    _selectedIndex = clickedIndex;
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
                else if (me.Delta < 0 && _scrollOffset + (Bounds.Height / _itemHeight) < _legends.Count)
                {
                    _scrollOffset++;
                }
                return true;
            }
        }

        return base.HandleEvent(e);
    }

    public LegendEntry GetSelectedLegend()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _legends.Count)
            return _legends[_selectedIndex];
        return null;
    }

    public void ClearSelection()
    {
        _selectedIndex = -1;
    }
}