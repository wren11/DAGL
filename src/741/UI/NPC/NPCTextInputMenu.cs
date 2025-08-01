using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using Size = System.Drawing.Size;

namespace DarkAges.Library.UI.NPC;

public class NPCTextInputMenu : NPCMenu
{
    private TextEditControlPane _inputControl;
    private string _prompt = "Enter text:";
    private string _defaultValue = "";

    public event EventHandler<TextInputEventArgs> TextSubmitted;

    public NPCTextInputMenu()
    {
        _inputControl = new TextEditControlPane();
        _inputControl.Position = new Point(110, 200);
        _inputControl.Size = new Size(280, 25);
        AddChild(_inputControl);
    }

    public void SetPrompt(string prompt)
    {
        _prompt = prompt ?? "Enter text:";
    }

    public void SetDefaultValue(string defaultValue)
    {
        _defaultValue = defaultValue ?? "";
        _inputControl.Text = _defaultValue;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || !_isVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        spriteBatch.FillRectangle(_menuBounds, _backgroundColor);
        spriteBatch.DrawRectangle(_menuBounds, _borderColor);

        //spriteBatch.DrawString(font, _prompt, _menuBounds.X + 10, _menuBounds.Y + 20, System.Drawing.Color.Yellow);

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !_isVisible) return false;

        if (e is KeyEvent keyEvent)
        {
            if (keyEvent.Type == EventType.KeyDown)
            {
                if (keyEvent.Key == Silk.NET.Input.Key.Enter)
                {
                    var text = _inputControl.Text;
                    TextSubmitted?.Invoke(this, new TextInputEventArgs(text));
                    return true;
                }
                else if (keyEvent.Key == Silk.NET.Input.Key.Escape)
                {
                    Hide();
                    return true;
                }
            }
        }

        return _inputControl.HandleEvent(e) || base.HandleEvent(e);
    }

    public string GetInputText()
    {
        return _inputControl.Text;
    }

    public void ClearInput()
    {
        _inputControl.Text = "";
    }
}