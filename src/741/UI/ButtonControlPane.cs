using System;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using System.Drawing;

namespace DarkAges.Library.UI;

public class ButtonControlPane : ControlPane
{
    private string _text = "";
    private SimpleFont _font;
    private bool _isPressed;
    private bool _isHovered;
    private Color _normalColor = Color.White;
    private Color _hoverColor = Color.LightGray;
    private Color _pressedColor = Color.DarkGray;
    private Color _textColor = Color.Black;

    public string Text
    {
        get => _text;
        set => _text = value ?? "";
    }

    public int State => _isPressed ? 2 : _isHovered ? 1 : 0;

    public event EventHandler Click;

    public ButtonControlPane()
    {
        _font = FontManager.GetFont("default") as SimpleFont;
        if (_font == null)
        {
            throw new InvalidOperationException("Default font not found");
        }
    }

    public void SetText(string text)
    {
        _text = text ?? "";
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null) return;

        var backgroundColor = _isPressed ? _pressedColor : (_isHovered ? _hoverColor : _normalColor);
        spriteBatch.FillRectangle(Bounds, backgroundColor);
        spriteBatch.DrawRectangle(Bounds, Color.Black);

        if (_font != null && !string.IsNullOrEmpty(_text))
        {
            var textSize = _font.MeasureString(_text);
            var textX = Bounds.X + (Bounds.Width - textSize.X) / 2;
            var textY = Bounds.Y + (Bounds.Height - _font.LineHeight) / 2;
            spriteBatch.DrawString(_font, _text, new DarkAges.Library.Graphics.Vector2(textX, textY), _textColor);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        if (e is MouseEvent mouseEvent)
        {
            var isInBounds = Bounds.Contains(mouseEvent.X, mouseEvent.Y);

            switch (mouseEvent.Type)
            {
                case EventType.MouseMove:
                    _isHovered = isInBounds;
                    return isInBounds;

                case EventType.MouseDown:
                    if (mouseEvent.Button == MouseButton.Left && isInBounds)
                    {
                        _isPressed = true;
                        return true;
                    }
                    break;

                case EventType.MouseUp:
                    if (mouseEvent.Button == MouseButton.Left)
                    {
                        if (_isPressed && isInBounds)
                        {
                            Click?.Invoke(this, EventArgs.Empty);
                        }
                        _isPressed = false;
                        return isInBounds;
                    }
                    break;
            }
        }

        return false;
    }
}