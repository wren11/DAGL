using DarkAges.Library.Core.Events;
using System;
using System.Drawing;
using System.Numerics;
using DarkAges.Library.Graphics;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.UI;

public class TextButtonExControlPane(string text) : ControlPane
{
    public string Text { get; set; } = text;
    public Point Position { get; internal set; }
    public bool Enabled { get; internal set; }
    public bool IsPressed { get; set; }
    public int State { get; set; } // 0 = normal, 1 = hover, 2 = pressed
    private SimpleFont? _font = FontManager.GetFont("default") as SimpleFont;

    public event Action<ControlPane> OnClick;
    public event EventHandler Click;

    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);

        if (_font != null)
        {
            var textSize = _font.MeasureString(Text);
            var x = Bounds.X + (Bounds.Width - textSize.Width) / 2;
            var y = Bounds.Y + (Bounds.Height - textSize.Height) / 2;
            _font.DrawString(spriteBatch, Text, new Vector2(x, y), Color.Black);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (base.HandleEvent(e)) return true;
            
        if (e is MouseEvent me)
        {
            if (Bounds.Contains(me.X, me.Y))
            {
                if (me.Type == EventType.MouseUp && me.Button == MouseButton.Left)
                {
                    OnClick?.Invoke(this);
                    Click?.Invoke(this, EventArgs.Empty);
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }
}