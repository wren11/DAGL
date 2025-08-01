namespace DarkAges.Library.UI.NPC;

public class NPCMenuItemEventArgs(NPCMenuItem menuItem) : EventArgs
{
    public NPCMenuItem MenuItem { get; } = menuItem ?? throw new ArgumentNullException(nameof(menuItem));
}