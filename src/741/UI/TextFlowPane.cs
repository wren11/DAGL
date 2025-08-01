using System;
using System.Drawing;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class TextFlowPane : ControlPane
{
    private string _text;
    private readonly List<string> _lines = [];
    private int _scrollOffset = 0;
    private SimpleFont _font;
    private Color _textColor;
    private bool _wordWrap;
    private const int LineHeight = 16;

    public TextFlowPane(Rectangle bounds)
    {
        _text = string.Empty;
        Bounds = bounds;
        LoadLayout();
        ProcessText();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_textflowpane.txt");
            var fontName = layout.GetString("FontName", "default");
            _font = FontManager.GetFont(fontName) as SimpleFont;
            _textColor = layout.GetColor("TextColor", Color.Black);
            _wordWrap = layout.GetBool("WordWrap", true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading text flow pane layout: {ex.Message}");
            _font = FontManager.GetFont("default") as SimpleFont;
            _textColor = Color.Black;
            _wordWrap = true;
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            ProcessText();
        }
    }

    private void ProcessText()
    {
        _lines.Clear();
        if (string.IsNullOrEmpty(_text) || _font == null)
            return;

        if (_wordWrap)
        {
            var words = _text.Split(' ');
            var currentLine = "";
            var maxWidth = Bounds.Width - 10; // Leave some padding

            foreach (var word in words)
            {
                var testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
                var lineWidth = _font.MeasureString(testLine).Width;

                if (lineWidth > maxWidth && currentLine.Length > 0)
                {
                    _lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (currentLine.Length > 0)
                _lines.Add(currentLine);
        }
        else
        {
            _lines.Add(_text);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || _font == null) return;

        var maxVisibleLines = Bounds.Height / LineHeight;
        var startLine = Math.Max(0, _scrollOffset);
        var endLine = Math.Min(_lines.Count, startLine + maxVisibleLines);

        for (var i = startLine; i < endLine; i++)
        {
            var y = Bounds.Y + 5 + (i - startLine) * LineHeight;
            spriteBatch.DrawString(_font, _lines[i], new System.Numerics.Vector2(Bounds.X + 5, y), _textColor);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        if (e is MouseEvent me && Bounds.Contains(me.X, me.Y))
        {
            if (me.Type == EventType.MouseWheel)
            {
                var maxVisibleLines = Bounds.Height / LineHeight;
                var maxScroll = Math.Max(0, _lines.Count - maxVisibleLines);

                if (me.Delta > 0 && _scrollOffset > 0)
                {
                    _scrollOffset--;
                    return true;
                }
                else if (me.Delta < 0 && _scrollOffset < maxScroll)
                {
                    _scrollOffset++;
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }
}