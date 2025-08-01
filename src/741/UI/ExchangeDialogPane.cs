using System;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.IO;
using DarkAges.Library.Graphics;
using DarkAges.Library.GameLogic;

namespace DarkAges.Library.UI;

public class ExchangeDialogPane : DialogPane
{
    private uint _exchangeId;
    private string _yourName;

    private TextEditControlPane _myIdLabel;
    private ExchangeItemListPane _myExchangeList;
    private TextEditControlPane _myMoneyInput;
        
    private TextEditControlPane _yourIdLabel;
    private ExchangeItemListPane _yourExchangeList;
    private TextEditControlPane _yourMoneyLabel;

    private TextButtonExControlPane _okButton;
    private TextButtonExControlPane _cancelButton;
        
    private CheckBoxControlPane _yourAckIndicator;
    private ImagePane _backgroundImage;
    private GraphicsDevice _graphicsDevice;

    private Rectangle _titleRect, _textRect;

    public event EventHandler<uint> ExchangeAccepted;
    public event EventHandler ExchangeCancelled;

    public ExchangeDialogPane(byte[] data)
    {
        ParseInitialData(data);
            
        var layout = new LayoutFileParser("_nexch.txt");
            
        // Set background image from layout
        var backgroundName = layout.GetString("Noname", "Noname");
        try
        {
            _backgroundImage = null;
            var indexedImage = ImageLoader.LoadImage(backgroundName);
            if (indexedImage != null)
            {
                _backgroundImage = new ImagePane();
                _backgroundImage.SetImage(indexedImage, null);
            }
        }
        catch (Exception ex)
        {
            // Fallback to default background
            _backgroundImage = null;
            Console.WriteLine($"Error loading background image: {ex.Message}");
        }

        _titleRect = layout.GetRect("Title");
        _textRect = layout.GetRect("Text");

        _okButton = new TextButtonExControlPane("OK");
        _cancelButton = new TextButtonExControlPane("Cancel");
            
        // Wire up button events
        _okButton.Click += OnOkClicked;
        _cancelButton.Click += OnCancelClicked;
            
        layout.SetSection("MyID");
        _myIdLabel = new TextEditControlPane("MyID", layout.GetRect("Rect"), true);
            
        _myExchangeList = new ExchangeItemListPane(layout.GetRect("MyExchange"));

        layout.SetSection("MyMoney");
        _myMoneyInput = new TextEditControlPane("0", layout.GetRect("Rect"), false);

        layout.SetSection("YourID");
        _yourIdLabel = new TextEditControlPane(_yourName, layout.GetRect("Rect"), true);

        _yourExchangeList = new ExchangeItemListPane(layout.GetRect("YourExchange"));

        layout.SetSection("YourMoney");
        _yourMoneyLabel = new TextEditControlPane("0", layout.GetRect("Rect"), true);

        if (layout.HasKey("YourACK"))
        {
            // Implement checkbox control for acknowledgment
            var ackRect = layout.GetRect("YourACK");
            _yourAckIndicator = new CheckBoxControlPane("Accept", ackRect);
            _yourAckIndicator.CheckedChanged += OnAckChanged;
            AddChild(_yourAckIndicator);
        }
    }
        
    private void ParseInitialData(byte[] data)
    {
        if (data.Length < 2) return;
            
        var offset = 2;
        _exchangeId = BitConverter.ToUInt32(data, offset);
        offset += 4;
            
        var nameLength = data[offset++];
        _yourName = System.Text.Encoding.ASCII.GetString(data, offset, nameLength);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Render background
        if (_backgroundImage != null)
        {
            _backgroundImage.Render(spriteBatch);
        }
        else
        {
            spriteBatch.DrawRectangle(Bounds, Color.LightGray);
            spriteBatch.DrawRectangle(Bounds, Color.Black);
        }

        // Render title
        if (!string.IsNullOrEmpty(_yourName))
        {
        }

        // Render child controls
        _myIdLabel?.Render(spriteBatch);
        _myExchangeList?.Render(spriteBatch);
        _myMoneyInput?.Render(spriteBatch);
        _yourIdLabel?.Render(spriteBatch);
        _yourExchangeList?.Render(spriteBatch);
        _yourMoneyLabel?.Render(spriteBatch);
        _okButton?.Render(spriteBatch);
        _cancelButton?.Render(spriteBatch);
        _yourAckIndicator?.Render(spriteBatch);

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        // Handle child control events
        if (_myIdLabel?.HandleEvent(e) == true) return true;
        if (_myExchangeList?.HandleEvent(e) == true) return true;
        if (_myMoneyInput?.HandleEvent(e) == true) return true;
        if (_yourIdLabel?.HandleEvent(e) == true) return true;
        if (_yourExchangeList?.HandleEvent(e) == true) return true;
        if (_yourMoneyLabel?.HandleEvent(e) == true) return true;
        if (_okButton?.HandleEvent(e) == true) return true;
        if (_cancelButton?.HandleEvent(e) == true) return true;
        if (_yourAckIndicator?.HandleEvent(e) == true) return true;

        return base.HandleEvent(e);
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        if (_yourAckIndicator?.IsChecked == true)
        {
            ExchangeAccepted?.Invoke(this, _exchangeId);
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        ExchangeCancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnAckChanged(object sender, bool isChecked)
    {
        _okButton.IsEnabled = isChecked;
    }

    public void AddMyItem(Item item)
    {
        _myExchangeList?.AddItem(item);
    }

    public void AddYourItem(Item item)
    {
        _yourExchangeList?.AddItem(item);
    }

    public void SetMyMoney(int amount)
    {
        _myMoneyInput.Text = amount.ToString();
    }

    public void SetYourMoney(int amount)
    {
        _yourMoneyLabel.Text = amount.ToString();
    }

    public void SetAcknowledged(bool acknowledged)
    {
        _yourAckIndicator?.SetChecked(acknowledged);
    }
}