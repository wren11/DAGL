using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.NPC;

public class NPC_Merchant_TextInputMenu : NPCTextInputMenu
{
    private int _itemId;
    private int _maxQuantity;
    private int _price;

    public event EventHandler<MerchantPurchaseEventArgs> PurchaseRequested;

    public void SetPurchaseContext(int itemId, int maxQuantity, int price)
    {
        _itemId = itemId;
        _maxQuantity = maxQuantity;
        _price = price;
        SetPrompt($"Enter quantity to purchase (Max: {maxQuantity}, Price: {price} each):");
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (e is KeyEvent keyEvent)
        {
            if (keyEvent.Type == EventType.KeyDown)
            {
                if (keyEvent.Key == Silk.NET.Input.Key.Enter)
                {
                    HandlePurchaseRequest();
                    return true;
                }
                else if (keyEvent.Key == Silk.NET.Input.Key.Escape)
                {
                    Hide();
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }

    private void HandlePurchaseRequest()
    {
        var inputText = GetInputText();
        if (int.TryParse(inputText, out var quantity))
        {
            if (quantity > 0 && quantity <= _maxQuantity)
            {
                var purchase = new MerchantPurchaseEventArgs(_itemId, quantity, _price * quantity);
                PurchaseRequested?.Invoke(this, purchase);
            }
        }
    }
}