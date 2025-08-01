using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.QuantityInput;

public class QuantityInputEvent(string itemName, int maxQuantity, ImagePane itemImage = null)
        : Event(EventType.Custom)
{
    public string ItemName { get; set; } = itemName;
    public int MaxQuantity { get; set; } = maxQuantity;
    public int RequestedQuantity { get; set; }
    public ImagePane ItemImage { get; set; } = itemImage;
}