using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Region;

public class MenuRegion(int id, string name, string menuType) : Region(id, name, "menu")
{
    public string MenuType { get; set; } = menuType ?? throw new ArgumentNullException(nameof(menuType));
    public bool IsModal { get; set; }
    public bool CanClose { get; set; } = true;

    public override bool HandleEvent(Event e)
    {
        if (e is KeyEvent keyEvent && keyEvent.Key == Silk.NET.Input.Key.Escape && CanClose)
        {
            Exit();
            return true;
        }

        return base.HandleEvent(e);
    }
}