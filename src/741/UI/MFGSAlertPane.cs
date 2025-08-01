using System;
using System.Drawing;
using System.Numerics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class MFGSAlertPane : ControlPane
{
    private readonly string _title;
    private readonly SimpleFont _font;
    private readonly ButtonControlPane _okButton;
    private readonly ButtonControlPane _cancelButton;

    public event EventHandler AlertResult;

    public MFGSAlertPane()
    {
        _title = "";
        _font = FontManager.GetFont("default") as SimpleFont;
        
        _okButton = new ButtonControlPane();
        _okButton.SetText("OK");
        _okButton.Click += (s, e) => AlertResult?.Invoke(this, EventArgs.Empty);

        _cancelButton = new ButtonControlPane();
        _cancelButton.SetText("Cancel");
        _cancelButton.Click += (s, e) => AlertResult?.Invoke(this, EventArgs.Empty);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null || _font == null) return;

        // Draw background
        var bounds = GetBounds();
        spriteBatch.FillRectangle(bounds, Color.FromArgb(200, 0, 0, 0));
        spriteBatch.DrawRectangle(bounds, Color.FromArgb(255, 255, 215, 0));

        // Draw title
        var titlePosition = new DarkAges.Library.Graphics.Vector2(bounds.X + 10, bounds.Y + 10);
        spriteBatch.DrawString(_font, _title, titlePosition, Color.White);

        // Draw buttons
        _okButton.Render(spriteBatch);
        _cancelButton.Render(spriteBatch);
    }

    private Rectangle GetBounds()
    {
        var width = 300;
        var height = 150;
        return new Rectangle(
            (GraphicsDevice.Instance.Width - width) / 2,
            (GraphicsDevice.Instance.Height - height) / 2,
            width,
            height
        );
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_okButton.HandleEvent(e)) return true;
        if (_cancelButton.HandleEvent(e)) return true;

        return false;
    }
}