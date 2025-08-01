namespace DarkAges.Library.GameLogic;

public class ItemEventArgs(Item item, object? oldValue = null, object? newValue = null)
        : EventArgs
{
    public Item Item { get; } = item;
    public object? OldValue { get; } = oldValue;
    public object? NewValue { get; } = newValue;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}