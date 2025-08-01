using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI.Connection;

/// <summary>
/// Checkbox control for UI
/// </summary>
public class CheckBoxControlPane : ControlPane
{
    private bool isChecked;
    private string text;

    public bool IsChecked
    {
        get => isChecked;
        set
        {
            isChecked = value;
            OnCheckedChanged?.Invoke(this, value);
        }
    }

    public string Text
    {
        get => text;
        set => text = value;
    }

    public event Action<ControlPane, bool> OnCheckedChanged;

    public CheckBoxControlPane(string text)
    {
        this.text = text;
        this.isChecked = false;
        Size = new Size(200, 20);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        // Render checkbox
        var boxColor = isChecked ? Color.Green : Color.Gray;
        spriteBatch.FillRectangle(new Rectangle(Position.X, Position.Y, 16, 16), boxColor);
        spriteBatch.DrawRectangle(new Rectangle(Position.X, Position.Y, 16, 16), Color.White);

        // Render checkmark if checked
        if (isChecked)
        {
            //spriteBatch.DrawString(font, "✓", Position.X + 2, Position.Y, Color.White);
        }

        // Render text
        //spriteBatch.DrawString(font, text, Position.X + 20, Position.Y, Color.White);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible)
            return false;

        if (e is MouseEvent mouseEvent && mouseEvent.Type == EventType.MouseDown)
        {
            if (mouseEvent.X >= Position.X && mouseEvent.X <= Position.X + 16 &&
                mouseEvent.Y >= Position.Y && mouseEvent.Y <= Position.Y + 16)
            {
                IsChecked = !IsChecked;
                return true;
            }
        }

        return base.HandleEvent(e);
    }
}