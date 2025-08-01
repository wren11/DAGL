using System;
using System.Collections.Generic;
using DarkAges.Library.Graphics;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Core.Events;
using System.Drawing;
using System.Linq;

namespace DarkAges.Library.UI.ItemShop;

public class ServerItemMenuDialog : DialogPane
{
    private readonly List<ServerItem> _items;
    private readonly ListPane _itemListPane;
    private readonly TextPane _descriptionPane;
    private readonly LabelControlPane _titleLabel;
    private readonly ButtonControlPane _buyButton;
    private readonly ButtonControlPane _sellButton;
    private readonly ButtonControlPane _cancelButton;
    private readonly TextEditControlPane _quantityInput;
    private readonly LabelControlPane _quantityLabel;

    public event EventHandler<ItemTransactionEventArgs> ItemTransactionRequested = delegate { };

    public ServerItemMenuDialog()
    {
        var font = FontManager.GetFont("default") as SimpleFont;
        if (font == null)
        {
            throw new InvalidOperationException("Default font not found");
        }

        _items = [];

        // Create title label
        _titleLabel = new LabelControlPane(font);
        _titleLabel.Text = "Shop";

        // Create item list
        _itemListPane = new ListPane(font);
        _itemListPane.Title = "Available Items";
        _itemListPane.OnItemSelected += OnItemSelected;

        // Create description pane
        _descriptionPane = new TextPane(font);
        _descriptionPane.Text = "Select an item to view its description.";
        _descriptionPane.IsMultiline = true;
        _descriptionPane.IsWordWrap = true;

        // Create quantity input
        _quantityLabel = new LabelControlPane(font);
        _quantityLabel.Text = "Quantity:";

        _quantityInput = new TextEditControlPane();
        _quantityInput.Text = "1";
        _quantityInput.IsNumeric = true;
        _quantityInput.MinValue = 1;
        _quantityInput.MaxValue = 99;

        // Create buttons
        _buyButton = new ButtonControlPane();
        _buyButton.SetText("Buy");
        _buyButton.Click += OnBuyButtonClick;

        _sellButton = new ButtonControlPane();
        _sellButton.SetText("Sell");
        _sellButton.Click += OnSellButtonClick;

        _cancelButton = new ButtonControlPane();
        _cancelButton.SetText("Cancel");
        _cancelButton.Click += OnCancelButtonClick;

        // Add controls
        AddChild(_titleLabel);
        AddChild(_itemListPane);
        AddChild(_descriptionPane);
        AddChild(_quantityLabel);
        AddChild(_quantityInput);
        AddChild(_buyButton);
        AddChild(_sellButton);
        AddChild(_cancelButton);

        // Set initial layout
        SetLayout();
    }

    public ServerItemMenuDialog(byte[]? packet) : this()
    {
        ParsePacket(packet);
    }

    private void ParsePacket(byte[]? packet)
    {
        // Implement packet parsing logic here based on game client analysis
        // For now, let's add some dummy items for testing
        SetItems(new List<ServerItem>
        {
            new ServerItem { Name = "Health Potion", Description = "Restores 50 health.", Price = 50, IsStackable = true, MaxQuantity = 10 },
            new ServerItem { Name = "Mana Potion", Description = "Restores 30 mana.", Price = 40, IsStackable = true, MaxQuantity = 10 },
            new ServerItem { Name = "Leather Armor", Description = "Basic leather armor.", Price = 100, IsStackable = false },
        });
    }

    private void SetLayout()
    {
        var padding = 10;
        var buttonWidth = 80;
        var buttonHeight = 30;
        var listWidth = 200;
        var descriptionWidth = 300;

        var bounds = GetBounds();
        var startX = bounds.X + padding;
        var startY = bounds.Y + padding;

        // Title
        _titleLabel.Bounds = new Rectangle(startX, startY, bounds.Width - 2 * padding, 30);
        startY += 40;

        // Item list
        _itemListPane.Bounds = new Rectangle(startX, startY, listWidth, bounds.Height - startY - buttonHeight - 2 * padding);

        // Description
        var descX = startX + listWidth + padding;
        _descriptionPane.Bounds = new Rectangle(descX, startY, descriptionWidth, bounds.Height - startY - buttonHeight - 2 * padding);

        // Quantity input
        var quantityY = bounds.Bottom - buttonHeight - padding;
        _quantityLabel.Bounds = new Rectangle(startX, quantityY, 60, buttonHeight);
        _quantityInput.Bounds = new Rectangle(startX + 70, quantityY, 50, buttonHeight);

        // Buttons
        var buttonY = bounds.Bottom - buttonHeight - padding;
        var buttonSpacing = (descriptionWidth - 3 * buttonWidth) / 4;
        var buttonX = descX + buttonSpacing;

        _buyButton.Bounds = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
        buttonX += buttonWidth + buttonSpacing;

        _sellButton.Bounds = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
        buttonX += buttonWidth + buttonSpacing;

        _cancelButton.Bounds = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
    }

    private Rectangle GetBounds()
    {
        var width = 600;
        var height = 400;
        return new Rectangle(
            (GraphicsDevice.Instance.Width - width) / 2,
            (GraphicsDevice.Instance.Height - height) / 2,
            width,
            height
        );
    }

    public void SetItems(List<ServerItem> items)
    {
        _items.Clear();
        _items.AddRange(items);

        _itemListPane.ClearItems();
        foreach (var item in items)
        {
            _itemListPane.AddItem(item.Name);
        }
    }

    private void OnItemSelected(object? sender, int selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < _items.Count)
        {
            var item = _items[selectedIndex];
            _descriptionPane.Text = GetItemDescription(item);
        }
    }

    private string GetItemDescription(ServerItem item)
    {
        return $"{item.Name}\n\n" +
               $"Price: {item.Price} gold\n" +
               $"Description: {item.Description}\n\n" +
               (item.IsStackable ? $"Max Stack: {item.MaxQuantity}\n" : "") +
               (item.Requirements.Count > 0 ? "\nRequirements:\n" + string.Join("\n", item.Requirements.Select(r => $"- {r.Key}: {r.Value}")) : "");
    }

    private void OnBuyButtonClick(object? sender, EventArgs e)
    {
        var selectedIndex = _itemListPane.GetSelectedIndex();
        if (selectedIndex < 0 || selectedIndex >= _items.Count) return;

        var item = _items[selectedIndex];
        if (int.TryParse(_quantityInput.Text, out var quantity))
        {
            ItemTransactionRequested?.Invoke(this, new ItemTransactionEventArgs(
                item,
                quantity,
                ItemTransactionType.Buy
            ));
        }
    }

    private void OnSellButtonClick(object? sender, EventArgs e)
    {
        var selectedIndex = _itemListPane.GetSelectedIndex();
        if (selectedIndex < 0 || selectedIndex >= _items.Count) return;

        var item = _items[selectedIndex];
        if (int.TryParse(_quantityInput.Text, out var quantity))
        {
            ItemTransactionRequested?.Invoke(this, new ItemTransactionEventArgs(
                item,
                quantity,
                ItemTransactionType.Sell
            ));
        }
    }

    private void OnCancelButtonClick(object? sender, EventArgs e)
    {
        Hide();
    }

    public override void Show()
    {
        _itemListPane.ClearItems();
        _descriptionPane.Text = "Select an item to view its description.";
        _quantityInput.Text = "1";
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }
}

public class ServerItem
{
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public bool IsStackable { get; set; }
    public int MaxQuantity { get; set; }
    public Dictionary<string, int> Requirements { get; set; } = [];
}