using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using Color = System.Drawing.Color;
using Size = DarkAges.Library.Graphics.Size;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.UI;

public class IMECandidatePane : ControlPane
{
    private int _imeX = -1;
    private int _imeY = -1;
    private bool _show = false;
    private int _candidateCount = 0;
    private int _selectedCandidate = 0;
    private readonly List<string> _candidates = [];
    private Color _backgroundColor;
    private Color _borderColor;
    private Color _textColor;
    private Color _selectedColor;
    private SimpleFont? _font;
    private int _lineHeight;
    private int _hMargin;

    public IMECandidatePane()
    {
        LoadLayout();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_imecandidate.txt");
            _backgroundColor = layout.GetColor("BackgroundColor", Color.Black);
            _borderColor = layout.GetColor("BorderColor", Color.White);
            _textColor = layout.GetColor("TextColor", Color.White);
            _selectedColor = layout.GetColor("SelectedColor", Color.Yellow);
            var fontName = layout.GetString("FontName", "default");
            _font = FontManager.GetFont(fontName) as SimpleFont;
            _lineHeight = layout.GetInt("LineHeight", 14);
            _hMargin = layout.GetInt("HMargin", 4);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading IME candidate layout: {ex.Message}");
            _backgroundColor = Color.Black;
            _borderColor = Color.White;
            _textColor = Color.White;
            _selectedColor = Color.Yellow;
            _font = FontManager.GetFont("default") as SimpleFont;
            _lineHeight = 14;
            _hMargin = 4;
        }
    }

    public new void SetPosition(int x, int y)
    {
        _imeX = x;
        _imeY = y;
    }

    public void ShowCandidates(List<string> candidates, int selected)
    {
        _candidates.Clear();
        _candidates.AddRange(candidates);
        _selectedCandidate = selected;
        _candidateCount = candidates.Count;
        _show = _candidateCount > 0;
        UpdatePaneBounds();
    }
        
    public void Hide()
    {
        _show = false;
    }

    private void UpdatePaneBounds()
    {
        if (_imeX < 0 || !_show) return;

        var maxChars = 0;
        foreach (var cand in _candidates)
        {
            if (cand.Length > maxChars)
                maxChars = cand.Length;
        }

        var charWidth = _font != null ? (int)_font.MeasureString("A").X : 8;
        var width = charWidth * (maxChars + 3) + _hMargin;
        var height = _candidateCount * _lineHeight + 2;

        var x = _imeX;
        var y = _imeY;

        if (x + width >= 640)
        {
            x = 640 - width;
        }

        if (_imeY - _lineHeight < height)
            y = _imeY + _lineHeight;
        else
            y = _imeY - height - _lineHeight;

        Position = new Point(x, y);
        Size = new Size(width, height);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);

        if (!_show || _font == null) return;

        spriteBatch.DrawRectangle(Bounds, _backgroundColor);
        spriteBatch.DrawRectangle(Bounds, _borderColor);

        for (var i = 0; i < _candidateCount; i++)
        {
            var color = i == _selectedCandidate ? _selectedColor : _textColor;
            spriteBatch.DrawString(_font, _candidates[i], new Vector2(Bounds.X + _hMargin, Bounds.Y + i * _lineHeight), color);
        }
    }
}