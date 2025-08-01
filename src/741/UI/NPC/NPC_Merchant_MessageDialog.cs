using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using System.Drawing;

namespace DarkAges.Library.UI.NPC;

public class NPC_Merchant_MessageDialog : ControlPane
{
    private readonly string _message;
    private readonly SimpleFont _font;
    private readonly List<string> _wrappedText;
    private readonly Rectangle _dialogBounds;
    private readonly Color _backgroundColor = Color.FromArgb(200, 0, 0, 0);
    private readonly Color _borderColor = Color.FromArgb(255, 255, 215, 0);
    private readonly Color _textColor = Color.White;

    private readonly ButtonControlPane _okButton;
    private readonly ButtonControlPane _cancelButton;

    public event EventHandler OkClicked;
    public event EventHandler CancelClicked;

    public NPC_Merchant_MessageDialog(string message, SimpleFont font, Rectangle bounds)
    {
        _message = message;
        _font = font;
        _dialogBounds = bounds;
        _wrappedText = WrapText(message, bounds.Width - 40, font);

        // Create buttons
        var buttonWidth = 80;
        var buttonHeight = 30;
        var buttonY = bounds.Bottom - buttonHeight - 10;

        _okButton = new ButtonControlPane();
        _okButton.SetText("OK");
        _okButton.Bounds = new Rectangle(bounds.X + bounds.Width / 2 - buttonWidth - 10, buttonY, buttonWidth, buttonHeight);
        _okButton.Click += (s, e) => OkClicked?.Invoke(this, EventArgs.Empty);

        _cancelButton = new ButtonControlPane();
        _cancelButton.SetText("Cancel");
        _cancelButton.Bounds = new Rectangle(bounds.X + bounds.Width / 2 + 10, buttonY, buttonWidth, buttonHeight);
        _cancelButton.Click += (s, e) => CancelClicked?.Invoke(this, EventArgs.Empty);
    }

    private List<string> WrapText(string text, int maxWidth, SimpleFont font)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            var size = font.MeasureString(testLine);

            if (size.X <= maxWidth)
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
        if (!IsVisible || spriteBatch == null) return;

        // Draw dialog background and border
        spriteBatch.FillRectangle(_dialogBounds, _backgroundColor);
        spriteBatch.DrawRectangle(_dialogBounds, _borderColor);

        // Draw wrapped text
        var textY = _dialogBounds.Y + 20;
        foreach (var line in _wrappedText)
        {
            spriteBatch.DrawString(_font, line, new DarkAges.Library.Graphics.Vector2(_dialogBounds.X + 20, textY), _textColor);
            textY += _font.LineHeight + 5;
        }

        // Draw buttons
        _okButton.Render(spriteBatch);
        _cancelButton.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_okButton.HandleEvent(e)) return true;
        if (_cancelButton.HandleEvent(e)) return true;

        return false;
    }
}