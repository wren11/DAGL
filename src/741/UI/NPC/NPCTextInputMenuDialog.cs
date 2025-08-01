using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public class NPCTextInputMenuDialog : ControlPane
{
    private NPCTextInputMenu _inputMenu;
    private ImagePane _backgroundImage;
    private Rectangle _dialogBounds;
    private bool _isModal;
    private bool _canClose;

    public event EventHandler<TextInputEventArgs> TextSubmitted;

    public NPCTextInputMenuDialog()
    {
        InitializeDialog();
    }

    private void InitializeDialog()
    {
        _inputMenu = new NPCTextInputMenu();
        _dialogBounds = new Rectangle(50, 50, 500, 300);
        _isModal = true;
        _canClose = true;

        _inputMenu.TextSubmitted += (s, e) => HandleTextSubmitted(e);

        AddChild(_inputMenu);
    }

    public void SetPrompt(string prompt)
    {
        _inputMenu.SetPrompt(prompt);
    }

    public void SetDefaultValue(string defaultValue)
    {
        _inputMenu.SetDefaultValue(defaultValue);
    }

    public void AddMenuItem(NPCMenuItem item)
    {
        _inputMenu.AddMenuItem(item);
    }

    public void AddMenuItem(string text, Action action = null, int id = 0)
    {
        var item = new NPCMenuItem(text, action, id);
        _inputMenu.AddMenuItem(item);
    }

    public new void SetBounds(Rectangle bounds)
    {
        _dialogBounds = bounds;
        _inputMenu.SetBounds(new Rectangle(bounds.X + 10, bounds.Y + 10, bounds.Width - 20, bounds.Height - 20));
    }

    public void SetModal(bool modal)
    {
        _isModal = modal;
    }

    public void SetCanClose(bool canClose)
    {
        _canClose = canClose;
    }

    public void ShowDialog()
    {
        Show();
        _inputMenu.Show();
    }

    private void HandleTextSubmitted(TextInputEventArgs e)
    {
        TextSubmitted?.Invoke(this, e);
        if (_isModal)
        {
            Hide();
        }
    }

    public string GetInputText()
    {
        return _inputMenu.GetInputText();
    }

    public void ClearInput()
    {
        _inputMenu.ClearInput();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;

        if (_backgroundImage != null)
        {
            //spriteBatch.DrawImage(_backgroundImage, _dialogBounds.X, _dialogBounds.Y);
        }
        else
        {
            spriteBatch.FillRectangle(_dialogBounds, Color.FromArgb(220, 0, 0, 0));
            spriteBatch.DrawRectangle(_dialogBounds, Color.White);
        }

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (e is KeyEvent keyEvent)
        {
            if (keyEvent.Type == EventType.KeyDown)
            {
                if (keyEvent.Key == Silk.NET.Input.Key.Escape && _canClose)
                {
                    Hide();
                    return true;
                }
            }
        }

        return _inputMenu.HandleEvent(e) || base.HandleEvent(e);
    }
}