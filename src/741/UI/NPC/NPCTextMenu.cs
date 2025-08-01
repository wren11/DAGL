using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public class NPCTextMenu : NPCMenu
{
    private string _title = "";
    private string _message = "";

    public NPCTextMenu()
    {
        _menuBounds = new Rectangle(100, 100, 400, 300);
    }

    public void SetTitle(string title)
    {
        _title = title ?? "";
    }

    public void SetMessage(string message)
    {
        _message = message ?? "";
    }

    public string GetTitle()
    {
        return _title;
    }

    public string GetMessage()
    {
        return _message;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || !_isVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        spriteBatch.FillRectangle(_menuBounds, _backgroundColor);
        spriteBatch.DrawRectangle(_menuBounds, _borderColor);

        var currentY = _menuBounds.Y + 10;

        if (!string.IsNullOrEmpty(_title))
        {
            //spriteBatch.DrawString(font, _title, _menuBounds.X + 10, currentY, System.Drawing.Color.Yellow);
            //currentY += font.Height + 10;
        }

        if (!string.IsNullOrEmpty(_message))
        {
            //var lines = WrapText(_message, _menuBounds.Width - 20, font);
            //foreach (var line in lines)
            //{
            //    spriteBatch.DrawString(font, line, _menuBounds.X + 10, currentY, _textColor);
            //    currentY += font.Height + 2;
            //}
            currentY += 10;
        }

        var itemHeight = 25;
        var startY = currentY;

        for (var i = 0; i < _menuItems.Count; i++)
        {
            var item = _menuItems[i];
            var itemRect = new Rectangle(_menuBounds.X + 5, startY + i * itemHeight, _menuBounds.Width - 10, itemHeight - 2);

            if (i == _selectedIndex)
            {
                spriteBatch.FillRectangle(itemRect, _selectedColor);
            }

            var textColor = i == _selectedIndex ? System.Drawing.Color.Yellow : _textColor;
            //spriteBatch.DrawString(font, item.Text, itemRect.X + 5, itemRect.Y + 5, textColor);
        }
    }

    private List<string> WrapText(string text, int maxWidth, SimpleFont font)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
            if (font.MeasureString(testLine).Width <= maxWidth)
            {
                currentLine = testLine;
            }
            else
            {
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    lines.Add(word);
                }
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine);
        }

        return lines;
    }
}