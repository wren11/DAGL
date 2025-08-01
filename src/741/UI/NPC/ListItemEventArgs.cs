namespace DarkAges.Library.UI.NPC;

public class ListItemEventArgs(string item, int index) : EventArgs
{
    public string Item { get; } = item ?? throw new ArgumentNullException(nameof(item));
    public int Index { get; } = index;
}