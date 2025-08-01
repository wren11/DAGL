using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public class NPCListMenuDialog : ControlPane
{
    private NPCListMenu _listMenu;
    private ImagePane _backgroundImage;
    private Rectangle _dialogBounds;
    private bool _isModal;
    private bool _canClose;

    public event EventHandler<ListItemEventArgs> ListItemSelected;

    public NPCListMenuDialog()
    {
        InitializeDialog();
    }

    private void InitializeDialog()
    {
        _listMenu = new NPCListMenu();
        _dialogBounds = new Rectangle(50, 50, 400, 500);
        _isModal = true;
        _canClose = true;

        _listMenu.ListItemSelected += (s, e) => ListItemSelected?.Invoke(this, e);

        AddChild(_listMenu);
    }

    public void AddListItem(string item)
    {
        _listMenu.AddListItem(item);
    }

    public void RemoveListItem(string item)
    {
        _listMenu.RemoveListItem(item);
    }

    public void ClearListItems()
    {
        _listMenu.ClearListItems();
    }

    public void AddMenuItem(NPCMenuItem item)
    {
        _listMenu.AddMenuItem(item);
    }

    public void AddMenuItem(string text, Action action = null, int id = 0)
    {
        var item = new NPCMenuItem(text, action, id);
        _listMenu.AddMenuItem(item);
    }

    public new void SetBounds(Rectangle bounds)
    {
        _dialogBounds = bounds;
        _listMenu.SetBounds(new Rectangle(bounds.X + 10, bounds.Y + 10, bounds.Width - 20, bounds.Height - 20));
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
        _listMenu.Show();
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

        return _listMenu.HandleEvent(e) || base.HandleEvent(e);
    }
}