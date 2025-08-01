using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using System.Drawing;

namespace DarkAges.Library.UI.NPC;

public class NPC_Merchant_TextMenu : NPCTextMenu
{
    private readonly List<MerchantItem> _merchantItems = [];
    private int _selectedItemIndex = -1;
    private int _playerGold = 0;

    public new event EventHandler<MerchantItemEventArgs> ItemSelected;
    public event EventHandler<MerchantItemEventArgs> ItemPurchased;

    public void AddMerchantItem(MerchantItem item)
    {
        if (item != null)
        {
            _merchantItems.Add(item);
        }
    }

    public void RemoveMerchantItem(MerchantItem item)
    {
        _merchantItems.Remove(item);
    }

    public void ClearMerchantItems()
    {
        _merchantItems.Clear();
        _selectedItemIndex = -1;
    }

    public void SetPlayerGold(int gold)
    {
        _playerGold = gold;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var font = FontManager.GetSimpleFont("default");

        spriteBatch.FillRectangle(_menuBounds, _backgroundColor);
        spriteBatch.DrawRectangle(_menuBounds, _borderColor);

        var currentY = _menuBounds.Y + 10;

        // Get title and message from base class
        var title = GetTitle();
        var message = GetMessage();

        if (!string.IsNullOrEmpty(title))
        {
            spriteBatch.DrawString(font, title, new DarkAges.Library.Graphics.Vector2(_menuBounds.X + 10, currentY), System.Drawing.Color.Yellow);
            currentY += font.LineHeight + 10;
        }

        if (!string.IsNullOrEmpty(message))
        {
            spriteBatch.DrawString(font, message, new DarkAges.Library.Graphics.Vector2(_menuBounds.X + 10, currentY), _textColor);
            currentY += font.LineHeight + 10;
        }

        spriteBatch.DrawString(font, $"Your Gold: {_playerGold}", new DarkAges.Library.Graphics.Vector2(_menuBounds.X + 10, currentY), System.Drawing.Color.Gold);
        currentY += font.LineHeight + 10;

        var itemHeight = 30;
        var startY = currentY;

        for (var i = 0; i < _merchantItems.Count; i++)
        {
            var item = _merchantItems[i];
            var itemRect = new Rectangle(_menuBounds.X + 5, startY + i * itemHeight, _menuBounds.Width - 10, itemHeight - 2);

            if (i == _selectedItemIndex)
            {
                spriteBatch.FillRectangle(itemRect, _selectedColor);
            }

            var textColor = i == _selectedItemIndex ? System.Drawing.Color.Yellow : _textColor;
            if (item.Price > _playerGold)
            {
                textColor = System.Drawing.Color.Red;
            }

            var itemText = $"{item.Name} - {item.Price} Gold";
            spriteBatch.DrawString(font, itemText, new DarkAges.Library.Graphics.Vector2(itemRect.X + 5, itemRect.Y + 5), textColor);

            if (item.Quantity > 0)
            {
                spriteBatch.DrawString(font, $"Stock: {item.Quantity}", new DarkAges.Library.Graphics.Vector2(itemRect.X + 200, itemRect.Y + 5), System.Drawing.Color.Gray);
            }
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (e is KeyEvent keyEvent)
        {
            if (keyEvent.Type == EventType.KeyDown)
            {
                switch (keyEvent.Key)
                {
                case Silk.NET.Input.Key.Up:
                    SelectPreviousMerchantItem();
                    return true;
                case Silk.NET.Input.Key.Down:
                    SelectNextMerchantItem();
                    return true;
                case Silk.NET.Input.Key.Enter:
                    PurchaseSelectedItem();
                    return true;
                case Silk.NET.Input.Key.Escape:
                    Hide();
                    return true;
                }
            }
        }

        if (e is MouseEvent mouseEvent)
        {
            if (mouseEvent.Button == MouseButton.Left && mouseEvent.Type == EventType.MouseDown)
            {
                var itemHeight = 30;
                var startY = _menuBounds.Y + 50;
                var relativeY = mouseEvent.Y - startY;
                var clickedIndex = relativeY / itemHeight;

                if (clickedIndex >= 0 && clickedIndex < _merchantItems.Count)
                {
                    SelectMerchantItem(clickedIndex);
                    PurchaseSelectedItem();
                    return true;
                }
            }
        }

        return false;
    }

    private void SelectMerchantItem(int index)
    {
        if (index >= 0 && index < _merchantItems.Count)
        {
            _selectedItemIndex = index;
            var item = _merchantItems[index];
            ItemSelected?.Invoke(this, new MerchantItemEventArgs(item));
        }
    }

    private void SelectNextMerchantItem()
    {
        if (_merchantItems.Count == 0) return;

        _selectedItemIndex = (_selectedItemIndex + 1) % _merchantItems.Count;
        var item = _merchantItems[_selectedItemIndex];
        ItemSelected?.Invoke(this, new MerchantItemEventArgs(item));
    }

    private void SelectPreviousMerchantItem()
    {
        if (_merchantItems.Count == 0) return;

        _selectedItemIndex = (_selectedItemIndex - 1 + _merchantItems.Count) % _merchantItems.Count;
        var item = _merchantItems[_selectedItemIndex];
        ItemSelected?.Invoke(this, new MerchantItemEventArgs(item));
    }

    private void PurchaseSelectedItem()
    {
        if (_selectedItemIndex >= 0 && _selectedItemIndex < _merchantItems.Count)
        {
            var item = _merchantItems[_selectedItemIndex];
                
            if (item.Price <= _playerGold && item.Quantity > 0)
            {
                _playerGold -= item.Price;
                item.Quantity--;
                ItemPurchased?.Invoke(this, new MerchantItemEventArgs(item));
            }
        }
    }

    public new MerchantItem GetSelectedItem()
    {
        if (_selectedItemIndex >= 0 && _selectedItemIndex < _merchantItems.Count)
        {
            return _merchantItems[_selectedItemIndex];
        }
        return null;
    }

    public List<MerchantItem> GetMerchantItems()
    {
        return [.._merchantItems];
    }
}