namespace DarkAges.Library.UI.NPC;

public class MerchantPurchaseEventArgs(int itemId, int quantity, int totalPrice) : EventArgs
{
    public int ItemId { get; } = itemId;
    public int Quantity { get; } = quantity;
    public int TotalPrice { get; } = totalPrice;
}