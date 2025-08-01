namespace DarkAges.Library.UI;

/// <summary>
/// Event data for inventory slot activation.
/// </summary>
public class InventorySlotEvent
{
    public int SlotIndex { get; set; }
    public bool IsShortcut { get; set; }
    public DateTime Timestamp { get; set; }
}