using System;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class TextEditControlPane : ControlPane
{
    private string _text = "";
    private bool _isPasswordField;
    private bool _isNumeric;
    private int _minValue;
    private int _maxValue;
    private bool _isReadOnly;
    private int _maxLength;
    private int _cursorPosition;
    private bool _isSelected;
    private SimpleFont _font;
    private GraphicsDevice _graphicsDevice;
    private string _label;

    public string Label
    {
        get => _label;
        set => _label = value;
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_isNumeric)
            {
                if (int.TryParse(value, out var numValue))
                {
                    numValue = Math.Clamp(numValue, _minValue, _maxValue);
                    _text = numValue.ToString();
                }
            }
            else
            {
                _text = value;
            }

            if (_maxLength > 0 && _text.Length > _maxLength)
            {
                _text = _text.Substring(0, _maxLength);
            }

            _cursorPosition = Math.Min(_cursorPosition, _text.Length);
        }
    }

    [Obsolete("Use IsPassword instead.")]
    public bool IsPasswordField
    {
        get => _isPasswordField;
        set => _isPasswordField = value;
    }

    public bool IsPassword
    {
        get => _isPasswordField;
        set => _isPasswordField = value;
    }

    public bool IsNumeric
    {
        get => _isNumeric;
        set => _isNumeric = value;
    }

    public int MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            if (_isNumeric && int.TryParse(_text, out var numValue) && numValue < _minValue)
            {
                Text = _minValue.ToString();
            }
        }
    }

    public int MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (_isNumeric && int.TryParse(_text, out var numValue) && numValue > _maxValue)
            {
                Text = _maxValue.ToString();
            }
        }
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => _isReadOnly = value;
    }

    public int MaxLength
    {
        get => _maxLength;
        set
        {
            _maxLength = value;
            if (_maxLength > 0 && _text.Length > _maxLength)
            {
                Text = _text.Substring(0, _maxLength);
            }
        }
    }

    public event EventHandler TextChanged;
    public event EventHandler<KeyPressEventArgs> KeyPress;

    public TextEditControlPane()
    {
        Initialize();
    }

    public TextEditControlPane(string text, Rectangle bounds, bool isReadOnly = false)
    {
        Initialize();
        Text = text;
        Bounds = bounds;
        IsReadOnly = isReadOnly;
    }

    private void Initialize()
    {
        _text = "";
        _isPasswordField = false;
        _isNumeric = false;
        _minValue = int.MinValue;
        _maxValue = int.MaxValue;
        _isReadOnly = false;
        _maxLength = 0;
        _cursorPosition = 0;
        _isSelected = false;
        _font = FontManager.GetFont("default") as SimpleFont;
        _graphicsDevice = GraphicsDevice.Instance;
        _label = "";
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null) return;

        // Draw label
        if (!string.IsNullOrEmpty(_label) && _font != null)
        {
            var labelPosition = new DarkAges.Library.Graphics.Vector2(Bounds.X, Bounds.Y - _font.LineHeight);
            spriteBatch.DrawString(_font, _label, labelPosition, Color.Black);
        }

        // Draw background
        spriteBatch.FillRectangle(Bounds, Color.White);
        spriteBatch.DrawRectangle(Bounds, Color.Black);

        // Draw text
        if (_font != null)
        {
            var displayText = _isPasswordField ? new string('*', _text.Length) : _text;
            var textPosition = new DarkAges.Library.Graphics.Vector2(Bounds.X + 5, Bounds.Y + (Bounds.Height - _font.LineHeight) / 2);
            spriteBatch.DrawString(_font, displayText, textPosition, Color.Black);

            // Draw cursor
            if (_isSelected && !_isReadOnly)
            {
                var cursorText = displayText.Substring(0, _cursorPosition);
                var cursorX = Bounds.X + 5 + _font.MeasureString(cursorText).X;
                var cursorY = Bounds.Y + 2;
                var cursorHeight = Bounds.Height - 4;
                spriteBatch.DrawLine(new DarkAges.Library.Graphics.Vector2(cursorX, cursorY), new DarkAges.Library.Graphics.Vector2(cursorX, cursorY + cursorHeight), Color.Black);
            }
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || _isReadOnly) return false;

        if (e is MouseEvent mouseEvent)
        {
            switch (mouseEvent.Type)
            {
                case EventType.MouseDown:
                    if (mouseEvent.Button == MouseButton.Left)
                    {
                        if (Bounds.Contains(mouseEvent.X, mouseEvent.Y))
                        {
                            _isSelected = true;
                            if (_font != null)
                            {
                                var clickX = mouseEvent.X - (Bounds.X + 5);
                                var displayText = _isPasswordField ? new string('*', _text.Length) : _text;
                                _cursorPosition = GetCharacterIndexAtPosition(displayText, clickX);
                            }
                            return true;
                        }
                        else
                        {
                            _isSelected = false;
                        }
                    }
                    break;
            }
        }
        else if (e is KeyEvent keyEvent && _isSelected)
        {
            switch (keyEvent.Type)
            {
                case EventType.KeyDown:
                    switch (keyEvent.Key)
                    {
                        case Silk.NET.Input.Key.Left:
                            if (_cursorPosition > 0)
                            {
                                _cursorPosition--;
                                return true;
                            }
                            break;
                        case Silk.NET.Input.Key.Right:
                            if (_cursorPosition < _text.Length)
                            {
                                _cursorPosition++;
                                return true;
                            }
                            break;
                        case Silk.NET.Input.Key.Home:
                            _cursorPosition = 0;
                            return true;
                        case Silk.NET.Input.Key.End:
                            _cursorPosition = _text.Length;
                            return true;
                        case Silk.NET.Input.Key.Delete:
                            if (_cursorPosition < _text.Length)
                            {
                                Text = _text.Remove(_cursorPosition, 1);
                                TextChanged?.Invoke(this, EventArgs.Empty);
                                return true;
                            }
                            break;
                        case Silk.NET.Input.Key.Backspace:
                            if (_cursorPosition > 0)
                            {
                                _cursorPosition--;
                                Text = _text.Remove(_cursorPosition, 1);
                                TextChanged?.Invoke(this, EventArgs.Empty);
                                return true;
                            }
                            break;
                    }
                    break;
                case EventType.KeyChar:
                    if (e is KeyCharEvent keyCharEvent)
                    {
                        var args = new KeyPressEventArgs(keyCharEvent.Char);
                        KeyPress?.Invoke(this, args);
                        if (!args.Handled)
                        {
                            if (_isNumeric && !char.IsDigit(keyCharEvent.Char) && keyCharEvent.Char != '-')
                                return true;

                            Text = _text.Insert(_cursorPosition, keyCharEvent.Char.ToString());
                            _cursorPosition++;
                            TextChanged?.Invoke(this, EventArgs.Empty);
                            return true;
                        }
                    }
                    break;
            }
        }

        return false;
    }

    private int GetCharacterIndexAtPosition(string text, float x)
    {
        if (_font == null) return 0;

        for (var i = 0; i <= text.Length; i++)
        {
            var subString = text.Substring(0, i);
            var width = _font.MeasureString(subString).X;
            if (width > x)
                return Math.Max(0, i - 1);
        }

        return text.Length;
    }
}