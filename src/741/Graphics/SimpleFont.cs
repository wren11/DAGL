using System;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;

namespace DarkAges.Library.Graphics;

public class SimpleFont(string name, int size = 12) : IDisposable
{
    private readonly Dictionary<char, FontGlyph> _glyphs = new();
    private Texture? _fontTexture;
    private int _lineHeight = size + 2;
    private bool _isDisposed;

    public string Name { get; private set; } = name;
    public int LineHeight => _lineHeight;

    public Size MeasureString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null!;

        var width = 0;
        var height = _lineHeight;
        var lineWidth = 0;
        var lineCount = 1;

        foreach (var c in text)
        {
            if (c == '\n')
            {
                width = Math.Max(width, lineWidth);
                lineWidth = 0;
                lineCount++;
                continue;
            }

            if (_glyphs.TryGetValue(c, out var glyph))
            {
                lineWidth += glyph.XAdvance;
            }
        }

        width = Math.Max(width, lineWidth);
        height = lineCount * _lineHeight;

        return new Size(width, height);
    }

    public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, System.Drawing.Color color)
    {
        if (string.IsNullOrEmpty(text) || _isDisposed)
            return;

        var currentPos = position;

        foreach (var c in text)
        {
            if (c == '\n')
            {
                currentPos.X = position.X;
                currentPos.Y += _lineHeight;
                continue;
            }

            if (_glyphs.TryGetValue(c, out var glyph))
            {
                var charRect = new Rectangle(
                    (int)(currentPos.X + glyph.XOffset),
                    (int)(currentPos.Y + glyph.YOffset),
                    glyph.Width,
                    glyph.Height
                );

                if (_fontTexture != null)
                {
                    var sourceRect = new Rectangle(glyph.X, glyph.Y, glyph.Width, glyph.Height);
                    spriteBatch.DrawTexture(_fontTexture, charRect, sourceRect, color);
                }
                    
                currentPos.X += glyph.XAdvance;
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
            
        _fontTexture?.Dispose();
        _glyphs.Clear();
        _isDisposed = true;
    }

    private struct FontGlyph
    {
        public char Character;
        public int X, Y, Width, Height;
        public int XOffset, YOffset;
        public int XAdvance;
    }
}