using System;

namespace DarkAges.Library.UI.ItemShop;

public class ItemTransactionEventArgs : EventArgs
{
    public ServerItem Item { get; }
    public int Quantity { get; }
    public ItemTransactionType TransactionType { get; }
    public bool TransactionSuccessful { get; set; }
    public bool IsBuyTransaction => TransactionType == ItemTransactionType.Buy;
    public int ItemId => Item.ItemId;
    public int Price => Item.Price;
    public byte MerchantId { get; set; }
    public ushort MenuId { get; set; }

    public ItemTransactionEventArgs(ServerItem item, int quantity, ItemTransactionType transactionType)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Quantity = quantity;
        TransactionType = transactionType;
    }
}