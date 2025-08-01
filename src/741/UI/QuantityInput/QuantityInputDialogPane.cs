using System;
using DarkAges.Library.Graphics;
using SpriteBatch = DarkAges.Library.Graphics.SpriteBatch;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI.QuantityInput;

public class QuantityInputDialogPane : ControlPane
{
    private TextEditControlPane _quantityBox;
    private TextButtonExControlPane _okButton;
    private TextButtonExControlPane _cancelButton;
    private TextButtonExControlPane _maxButton;
    private ImagePane _backgroundImage;
    private Rectangle _backgroundRect;
    private int _maxQuantity;
    private int _currentQuantity;
    private string _itemName;
    private ImagePane _itemImage;

    public event EventHandler<int> QuantityConfirmed;
    public event EventHandler QuantityCancelled;

    public QuantityInputDialogPane()
    {
        InitializeControls();
        LoadLayout();
    }

    private void InitializeControls()
    {
        _quantityBox = new TextEditControlPane();
        _okButton = new TextButtonExControlPane("OK");
        _cancelButton = new TextButtonExControlPane("Cancel");
        _maxButton = new TextButtonExControlPane("Max");

        _quantityBox.Position = new Point(150, 150);
        _quantityBox.Size = new Size(100, 20);
        _okButton.Position = new Point(100, 200);
        _cancelButton.Position = new Point(200, 200);
        _maxButton.Position = new Point(300, 200);

        _quantityBox.Text = "1";
        _quantityBox.TextChanged += (s, e) => ValidateQuantity();

        _okButton.Click += (s, e) => ConfirmQuantity();
        _cancelButton.Click += (s, e) => CancelQuantity();
        _maxButton.Click += (s, e) => SetMaxQuantity();

        AddChild(_quantityBox);
        AddChild(_okButton);
        AddChild(_cancelButton);
        AddChild(_maxButton);
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_quantitydlg.txt");
                
            var backgroundName = layout.GetString("Background", "quantity_bg");
            _backgroundImage = new ImagePane();
            _backgroundImage.SetImage(ImageLoader.LoadImage(backgroundName), null);
                
            _backgroundRect = layout.GetRect("Background", new Rectangle(200, 150, 400, 250));
                
            _quantityBox.Bounds = layout.GetRect("Quantity");
            _okButton.Bounds = layout.GetRect("OK");
            _cancelButton.Bounds = layout.GetRect("Cancel");
            _maxButton.Bounds = layout.GetRect("Max");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
            _backgroundRect = new Rectangle(200, 150, 400, 250);
        }
    }

    private void ValidateQuantity()
    {
        if (int.TryParse(_quantityBox.Text, out var quantity))
        {
            if (quantity < 1)
            {
                _quantityBox.Text = "1";
                _currentQuantity = 1;
            }
            else if (quantity > _maxQuantity)
            {
                _quantityBox.Text = _maxQuantity.ToString();
                _currentQuantity = _maxQuantity;
            }
            else
            {
                _currentQuantity = quantity;
            }
        }
        else
        {
            _quantityBox.Text = "1";
            _currentQuantity = 1;
        }
    }

    private void ConfirmQuantity()
    {
        if (int.TryParse(_quantityBox.Text, out var quantity))
        {
            QuantityConfirmed?.Invoke(this, quantity);
            Hide();
        }
    }

    private void CancelQuantity()
    {
        QuantityCancelled?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    private void SetMaxQuantity()
    {
        _quantityBox.Text = _maxQuantity.ToString();
        _currentQuantity = _maxQuantity;
    }

    public void ShowQuantityDialog(string itemName, int maxQuantity, ImagePane itemImage = null)
    {
        _itemName = itemName;
        _maxQuantity = maxQuantity;
        _itemImage = itemImage;
        _currentQuantity = 1;
        _quantityBox.Text = "1";
            
        Show();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        if (_backgroundImage != null)
        {
            _backgroundImage.Render(spriteBatch);
        }
        else
        {
            spriteBatch.DrawRectangle(_backgroundRect, System.Drawing.Color.White);
            spriteBatch.DrawRectangle(_backgroundRect, System.Drawing.Color.Black);
        }

        if (_itemImage != null)
        {
            _itemImage.Render(spriteBatch);
        }
            
        base.Render(spriteBatch);
    }
}