using System.Drawing;
using System.Numerics;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class LabelControlPane : ControlPane
{
    private string _text = "";
    private SimpleFont _font;
    private Color _textColor = Color.White;
    private bool _isMultiline = false;
    private bool _isWordWrap = false;
    private int _maxWidth = 0;
    private int _maxHeight = 0;
    private DarkAges.Library.Graphics.Vector2 _textPosition;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateTextPosition();
        }
    }

    public Color TextColor
    {
        get => _textColor;
        set => _textColor = value;
    }

    public bool IsMultiline
    {
        get => _isMultiline;
        set => _isMultiline = value;
    }

    public bool IsWordWrap
    {
        get => _isWordWrap;
        set => _isWordWrap = value;
    }

    public int MaxWidth
    {
        get => _maxWidth;
        set
        {
            _maxWidth = value;
            UpdateTextPosition();
        }
    }

    public int MaxHeight
    {
        get => _maxHeight;
        set
        {
            _maxHeight = value;
            UpdateTextPosition();
        }
    }

    public LabelControlPane(SimpleFont font)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _textPosition = new DarkAges.Library.Graphics.Vector2(0, 0);
    }

    private void UpdateTextPosition()
    {
        if (_font == null) return;

        var textSize = _font.MeasureString(_text);
        var x = Position.X;
        var y = Position.Y;

        if (_isMultiline && _isWordWrap && _maxWidth > 0)
        {
            // Handle word wrapping
            var lines = WrapText(_text, _maxWidth);
            textSize = _font.MeasureString(string.Join("\n", lines));
        }

        _textPosition = new DarkAges.Library.Graphics.Vector2(x, y);
    }

    private List<string> WrapText(string text, int maxWidth)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            var testSize = _font.MeasureString(testLine);

            if (testSize.X <= maxWidth)
            {
                currentLine = testLine;
            }
            else
            {
                if (currentLine.Length > 0)
                    lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine);

        return lines;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null || _font == null) return;

        if (_isMultiline && _isWordWrap && _maxWidth > 0)
        {
            var lines = WrapText(_text, _maxWidth);
            var y = _textPosition.Y;

            foreach (var line in lines)
            {
                spriteBatch.DrawString(_font, line, new DarkAges.Library.Graphics.Vector2(_textPosition.X, y), _textColor);
                y += _font.LineHeight;

                if (_maxHeight > 0 && y - _textPosition.Y > _maxHeight)
                    break;
            }
        }
        else
        {
            spriteBatch.DrawString(_font, _text, _textPosition, _textColor);
        }
    }
}