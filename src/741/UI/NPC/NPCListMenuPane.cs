using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public class NPCListMenuPane : ControlPane
{
    private NPCListMenu _listMenu;
    private Rectangle _paneBounds;

    public event EventHandler<ListItemEventArgs> ListItemSelected;

    public NPCListMenuPane()
    {
        InitializePane();
    }

    private void InitializePane()
    {
        _listMenu = new NPCListMenu();
        _paneBounds = new Rectangle(0, 0, 300, 400);

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
        _paneBounds = bounds;
        _listMenu.SetBounds(new Rectangle(bounds.X + 5, bounds.Y + 5, bounds.Width - 10, bounds.Height - 10));
    }

    public void ShowList()
    {
        Show();
        _listMenu.Show();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;

        spriteBatch.FillRectangle(_paneBounds, Color.FromArgb(180, 0, 0, 0));
        spriteBatch.DrawRectangle(_paneBounds, Color.Gray);

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        return _listMenu.HandleEvent(e) || base.HandleEvent(e);
    }
}