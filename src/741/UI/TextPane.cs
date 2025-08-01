using System.Drawing;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using System.Numerics;

namespace DarkAges.Library.UI;

public class TextPane : ControlPane
{
    private SimpleFont _font;
    private string _text;
    private Color _textColor;
    private bool _isMultiline;
    private bool _isWordWrap;
    private int _maxWidth;
    private int _maxHeight;
    private List<string> _wrappedLines;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateWrappedLines();
        }
    }

    public Color TextColor
    {
        get => _textColor;
        set => _textColor = value;
    }

    public SimpleFont Font
    {
        get => _font;
        set
        {
            if (value != null)
            {
                _font = value;
                UpdateWrappedLines();
            }
        }
    }

    public bool IsMultiline
    {
        get => _isMultiline;
        set
        {
            _isMultiline = value;
            UpdateWrappedLines();
        }
    }

    public bool IsWordWrap
    {
        get => _isWordWrap;
        set
        {
            _isWordWrap = value;
            UpdateWrappedLines();
        }
    }

    public TextPane(SimpleFont font)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _text = "";
        _textColor = Color.White;
        _isMultiline = true;
        _isWordWrap = true;
        _maxWidth = 0;
        _maxHeight = 0;
        _wrappedLines = [];
    }

    public TextPane(string text, SimpleFont font) : this(font)
    {
        Text = text;
    }

    public TextPane(string text, Rectangle bounds, SimpleFont font) : this(font)
    {
        Text = text;
        Bounds = bounds;
    }

    private void UpdateWrappedLines()
    {
        _wrappedLines.Clear();

        if (string.IsNullOrEmpty(_text))
            return;

        if (!_isMultiline)
        {
            _wrappedLines.Add(_text);
            return;
        }

        var lines = _text.Split('\n');
        foreach (var line in lines)
        {
            if (!_isWordWrap || _maxWidth <= 0)
            {
                _wrappedLines.Add(line);
                continue;
            }

            var words = line.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                var size = _font.MeasureString(testLine);

                if (size.X <= _maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (currentLine.Length > 0)
                        _wrappedLines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (currentLine.Length > 0)
                _wrappedLines.Add(currentLine);
        }

        if (_maxHeight > 0)
        {
            var maxLines = _maxHeight / _font.LineHeight;
            if (_wrappedLines.Count > maxLines)
            {
                _wrappedLines = _wrappedLines.Take(maxLines).ToList();
                if (maxLines > 0)
                {
                    var lastLine = _wrappedLines[maxLines - 1];
                    _wrappedLines[maxLines - 1] = lastLine + "...";
                }
            }
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null) return;

        var position = new DarkAges.Library.Graphics.Vector2(Bounds.X, Bounds.Y);
        foreach (var line in _wrappedLines)
        {
            spriteBatch.DrawString(_font, line, position, _textColor);
            position.Y += _font.LineHeight;
        }
    }
}