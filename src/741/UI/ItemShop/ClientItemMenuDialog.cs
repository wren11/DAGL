using System;
using System.Collections.Generic;
using DarkAges.Library.Graphics;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.ItemShop;

public class ClientItemMenuDialog : DialogPane
{
    private ushort _itemCount;
    public class ClientItemEntry
    {
        public byte ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public uint Quantity { get; set; }
    }
        
    private ClientItemEntry[] _items = [];
    private ushort _menuId;
    private int _selectedItemIndex = -1;
    private ListPane _itemListPane = null!;
    private DescPane _descriptionPane = null!;
    private TextEditControlPane _titleLabel = null!;
    private TextButtonExControlPane _sellButton = null!;
    private TextButtonExControlPane _cancelButton = null!;
    private TextEditControlPane _quantityInput = null!;
    private TextEditControlPane _quantityLabel = null!;

    public event EventHandler<ItemTransactionEventArgs> ItemTransactionRequested = delegate { };

    public ClientItemMenuDialog(byte[] packet)
    {
        ParsePacket(packet);
        InitializeUI();
    }

    private void ParsePacket(byte[] packet)
    {
        var offset = 2;
            
        _menuId = BitConverter.ToUInt16(packet, offset);
        offset += 2;
            
        _itemCount = packet[offset++];
            
        _items = new ClientItemEntry[_itemCount];
        for (var i = 0; i < _itemCount; i++)
        {
            _items[i] = new ClientItemEntry
            {
                ItemId = packet[offset++]
            };
                
            if (_menuId == 78)
            {
                _items[i].Quantity = BitConverter.ToUInt32(packet, offset);
                offset += 4;
            }
        }
    }

    private void InitializeUI()
    {
        Size = new System.Drawing.Size(400, 300);
        Position = new System.Drawing.Point(120, 90);

        _titleLabel = new TextEditControlPane("Your Items", 
            new System.Drawing.Rectangle(10, 10, 380, 20), true);
        AddChild(_titleLabel);

        _itemListPane = new ListPane(FontManager.GetFont("default") as SimpleFont);
        _itemListPane.Position = new System.Drawing.Point(10, 40);
        _itemListPane.Size = new System.Drawing.Size(200, 200);
        AddChild(_itemListPane);

        PopulateItemList();

        _descriptionPane = new DescPane();
        _descriptionPane.Position = new System.Drawing.Point(220, 40);
        _descriptionPane.Size = new System.Drawing.Size(170, 200);
        AddChild(_descriptionPane);

        _quantityLabel = new TextEditControlPane("Quantity:", new System.Drawing.Rectangle(10, 220, 60, 20), true);
        AddChild(_quantityLabel);

        _quantityInput = new TextEditControlPane("1", new System.Drawing.Rectangle(80, 220, 50, 20), false);
        AddChild(_quantityInput);

        _sellButton = new TextButtonExControlPane("Sell");
        _sellButton.Position = new System.Drawing.Point(10, 250);
        _sellButton.Size = new System.Drawing.Size(60, 25);
        _sellButton.OnClick += OnSellClicked;
        AddChild(_sellButton);

        _cancelButton = new TextButtonExControlPane("Cancel");
        _cancelButton.Position = new System.Drawing.Point(330, 250);
        _cancelButton.Size = new System.Drawing.Size(60, 25);
        _cancelButton.OnClick += OnCancelClicked;
        AddChild(_cancelButton);

        if (_itemCount > 0)
        {
            ShowItemDescription(0);
        }
    }

    private void PopulateItemList()
    {
        for (var i = 0; i < _itemCount; i++)
        {
            var item = _items[i];
            var itemInfo = GetItemInfo(item.ItemId);
            var displayText = itemInfo.Name;
                
            if (_menuId == 78 && item.Quantity > 0)
            {
                displayText += $" (x{item.Quantity})";
            }
                
            var itemButton = new TextButtonExControlPane(displayText);
            itemButton.Position = new System.Drawing.Point(0, i * 25);
            itemButton.Size = new System.Drawing.Size(190, 23);
            itemButton.OnClick += (sender) => OnItemSelected(i);
                
            _itemListPane.AddChild(itemButton);
        }
    }

    private ItemInfo GetItemInfo(byte itemId)
    {
        // For now, create a placeholder item info since ItemMetaDescMan.GetItemMeta doesn't exist
        return new ItemInfo
        {
            Id = itemId,
            Name = $"Item {itemId}",
            Description = "This item has no description available.",
            SellPrice = (uint)(itemId * 10)
        };
    }

    private void OnItemSelected(int itemIndex)
    {
        _selectedItemIndex = itemIndex;
        if (itemIndex >= 0 && itemIndex < _itemCount)
        {
            ShowItemDescription(itemIndex);
        }
    }

    private void ShowItemDescription(int itemIndex)
    {
        var item = _items[itemIndex];
        var itemInfo = GetItemInfo(item.ItemId);
            
        var description = $"{itemInfo.Name}\n\n{itemInfo.Description}\n\nSell Price: {itemInfo.SellPrice} gold";
        if (_menuId == 78 && item.Quantity > 0)
        {
            description += $"\nQuantity: {item.Quantity}";
        }
            
        _descriptionPane.Show(description, new System.Drawing.Point(0, 0), _descriptionPane.Size);
    }

    private void OnSellClicked(ControlPane sender)
    {
        if (_selectedItemIndex >= 0 && _selectedItemIndex < _itemCount)
        {
            var item = _items[_selectedItemIndex];
            var itemInfo = GetItemInfo(item.ItemId);
                
            if (int.TryParse(_quantityInput.Text, out var quantity) && quantity > 0)
            {
                if (_menuId == 78 && item.Quantity > 0 && quantity > item.Quantity)
                {
                    quantity = (int)item.Quantity;
                }
                    
                var serverItem = new ServerItem
                {
                    ItemId = item.ItemId,
                    Name = itemInfo.Name,
                    Price = (int)itemInfo.SellPrice
                };

                var args = new ItemTransactionEventArgs(serverItem, quantity, ItemTransactionType.Sell);
                    
                ItemTransactionRequested?.Invoke(this, args);

                // TODO: The logic for what to do after a transaction is not clear.
                // Assuming for now that we close the dialog.
                Close(1);
            }
        }
    }

    private void OnCancelClicked(ControlPane sender)
    {
        Close(0);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        spriteBatch.DrawRectangle(Bounds, ColorRgb565.DarkGray);
        spriteBatch.DrawRectangle(new System.Drawing.Rectangle(Bounds.X + 1, Bounds.Y + 1, Bounds.Width - 2, Bounds.Height - 2), ColorRgb565.LightGray);

        base.Render(spriteBatch);
    }

    private class ItemInfo
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public uint SellPrice { get; set; }
    }

    public ushort MenuId => _menuId;
    public ushort ItemCount => _itemCount;
    public ClientItemEntry[] Items => _items;
}