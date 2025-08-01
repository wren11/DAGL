using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class CheckBoxControlPane : ControlPane
{
    private bool _isChecked;
    private string _text;
    private SimpleFont _font;
    private GraphicsDevice _graphicsDevice;

    public event EventHandler<bool> CheckedChanged;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked != value)
            {
                _isChecked = value;
                CheckedChanged?.Invoke(this, _isChecked);
            }
        }
    }

    public CheckBoxControlPane(string text, Rectangle bounds)
    {
        _text = text;
        Bounds = bounds;
        CanFocus = true;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Render checkbox
        var checkboxSize = 16;
        var checkboxRect = new Rectangle(Bounds.X, Bounds.Y + 2, checkboxSize, checkboxSize);
            
        spriteBatch.DrawRectangle(checkboxRect, Color.White);
        spriteBatch.DrawRectangle(checkboxRect, Color.Black);

        if (_isChecked)
        {
        }

        // Render text
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        if (e is MouseEvent me && Bounds.Contains(me.X, me.Y) && me.Type == EventType.LButtonDown)
        {
            IsChecked = !IsChecked;
            return true;
        }

        return base.HandleEvent(e);
    }

    public void SetChecked(bool isChecked)
    {
        IsChecked = isChecked;
    }
}