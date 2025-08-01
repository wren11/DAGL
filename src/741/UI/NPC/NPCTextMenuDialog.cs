using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public class NPCTextMenuDialog : ControlPane
{
    private NPCTextMenu _textMenu;
    private ImagePane _backgroundImage;
    private Rectangle _dialogBounds;
    private bool _isModal;
    private bool _canClose;

    public event EventHandler<NPCMenuItemEventArgs> MenuItemSelected;
    public event EventHandler<NPCMenuItemEventArgs> MenuItemClicked;

    public NPCTextMenuDialog()
    {
        InitializeDialog();
    }

    private void InitializeDialog()
    {
        _textMenu = new NPCTextMenu();
        _dialogBounds = new Rectangle(50, 50, 500, 400);
        _isModal = true;
        _canClose = true;

        _textMenu.ItemSelected += (s, e) => MenuItemSelected?.Invoke(this, e);
        _textMenu.ItemClicked += (s, e) => MenuItemClicked?.Invoke(this, e);

        AddChild(_textMenu);
    }

    public void SetTitle(string title)
    {
        _textMenu.SetTitle(title);
    }

    public void SetMessage(string message)
    {
        _textMenu.SetMessage(message);
    }

    public void AddMenuItem(NPCMenuItem item)
    {
        _textMenu.AddMenuItem(item);
    }

    public void AddMenuItem(string text, Action action = null, int id = 0)
    {
        var item = new NPCMenuItem(text, action, id);
        _textMenu.AddMenuItem(item);
    }

    public void ClearMenuItems()
    {
        _textMenu.ClearMenuItems();
    }

    public new void SetBounds(Rectangle bounds)
    {
        _dialogBounds = bounds;
        _textMenu.SetBounds(new Rectangle(bounds.X + 10, bounds.Y + 10, bounds.Width - 20, bounds.Height - 20));
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
        _textMenu.Show();
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

        return _textMenu.HandleEvent(e) || base.HandleEvent(e);
    }
}