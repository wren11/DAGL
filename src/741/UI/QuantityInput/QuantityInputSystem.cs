using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.QuantityInput;

public class QuantityInputSystem : ControlPane
{
    private QuantityInputDialogPane _quantityDialog;
    private int _lastConfirmedQuantity;

    public event EventHandler<int> QuantityInputConfirmed;
    public event EventHandler QuantityInputCancelled;

    public QuantityInputSystem()
    {
        _quantityDialog = new QuantityInputDialogPane();
        _quantityDialog.QuantityConfirmed += (s, quantity) => HandleQuantityConfirmed(quantity);
        _quantityDialog.QuantityCancelled += (s, e) => HandleQuantityCancelled();
    }

    public void ShowQuantityInput(string itemName, int maxQuantity, ImagePane itemImage = null)
    {
        _quantityDialog.ShowQuantityDialog(itemName, maxQuantity, itemImage);
    }

    private void HandleQuantityConfirmed(int quantity)
    {
        _lastConfirmedQuantity = quantity;
        QuantityInputConfirmed?.Invoke(this, quantity);
    }

    private void HandleQuantityCancelled()
    {
        QuantityInputCancelled?.Invoke(this, EventArgs.Empty);
    }

    public int GetLastConfirmedQuantity()
    {
        return _lastConfirmedQuantity;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _quantityDialog.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_quantityDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _quantityDialog?.Dispose();
    }
}