using System;
using System.Drawing;
using System.Text;
using DarkAges.Library.Core;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.Input;
using DarkAges.Library.UI.ItemShop;
using Silk.NET.Input;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI;

/// <summary>
/// Handles quantity input dialog for item transactions
/// </summary>
public class QuantityInputDialogPane : DialogPane
{
    private const int DEFAULT_WIDTH = 320;
    private const int DEFAULT_HEIGHT = 120;
    private const int BUTTON_WIDTH = 80;
    private const int BUTTON_HEIGHT = 25;
    private const int TEXT_PADDING = 10;

    private string? promptText;
    private int currentQuantity;
    private int maxQuantity;
    private int minQuantity;
    private EditablePaperPane? quantityInput;
    private Button? okButton;
    private Button? cancelButton;
    private Label? promptLabel;
    private bool quantityConfirmed;

    // Events
    public event Action<int>? QuantityConfirmed;
    public event Action? QuantityCancelled;

    public QuantityInputDialogPane()
    {
        InitializeDialog();
    }

    public QuantityInputDialogPane(string prompt, int maxQty, int minQty = 1)
    {
        InitializeDialog();
        SetPrompt(prompt);
        SetQuantityLimits(minQty, maxQty);
    }

    private void InitializeDialog()
    {
        // Set dialog properties
        SetSize(DEFAULT_WIDTH, DEFAULT_HEIGHT);
        SetTitle("Quantity Input");
        SetModal(true);

        // Initialize variables
        promptText = "Enter quantity:";
        currentQuantity = 1;
        maxQuantity = 999;
        minQuantity = 1;
        quantityConfirmed = false;
        quantityInput = null;
        okButton = null;
        cancelButton = null;
        promptLabel = null;
        QuantityConfirmed = null;
        QuantityCancelled = null;

        // Create UI elements
        CreateUIElements();

        // Set default position (center of screen)
        CenterOnScreen();
    }

    public new void CenterOnScreen()
    {
        base.CenterOnScreen();
    }

    public new void SetModal(bool isModal)
    {
        base.IsModal = isModal;
    }

    public new void SetTitle(string title)
    {
        base.Title = title;
    }

    private void CreateUIElements()
    {
        // Create prompt label
        promptLabel = new Label();
        promptLabel.Text = promptText;
        promptLabel.Location = new Point(TEXT_PADDING, TEXT_PADDING);
        promptLabel.Size = new Size(Width - TEXT_PADDING * 2, 20);
        AddChild(promptLabel);

        // Create quantity input
        quantityInput = new EditablePaperPane();
        quantityInput.Size = new Size(Width - TEXT_PADDING * 2, 25);
        quantityInput.Location = new Point(TEXT_PADDING, TEXT_PADDING + 25);
        quantityInput.Text = "1";
        quantityInput.MaxLength = 10;
        AddChild(quantityInput);

        // Create OK button
        okButton = new Button();
        okButton.Text = "OK";
        okButton.Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT);
        okButton.Location = new Point(Width - BUTTON_WIDTH * 2 - TEXT_PADDING * 2, Height - BUTTON_HEIGHT - TEXT_PADDING);
        okButton.Click += OnOKButtonClick;
        AddChild(okButton);

        // Create Cancel button
        cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT);
        cancelButton.Location = new Point(Width - BUTTON_WIDTH - TEXT_PADDING, Height - BUTTON_HEIGHT - TEXT_PADDING);
        cancelButton.Click += OnCancelButtonClick;
        AddChild(cancelButton);
    }

    public void SetPrompt(string prompt)
    {
        promptText = prompt ?? "Enter quantity:";
        if (promptLabel != null)
        {
            promptLabel.Text = promptText;
        }
    }

    public void SetQuantityLimits(int min, int max)
    {
        minQuantity = Math.Max(1, min);
        maxQuantity = Math.Max(minQuantity, max);
        currentQuantity = Math.Max(minQuantity, Math.Min(currentQuantity, maxQuantity));
            
        if (quantityInput != null)
        {
            quantityInput.Text = currentQuantity.ToString();
        }
    }

    public void SetCurrentQuantity(int quantity)
    {
        currentQuantity = Math.Max(minQuantity, Math.Min(quantity, maxQuantity));
        if (quantityInput != null)
        {
            quantityInput.Text = currentQuantity.ToString();
        }
    }

    public int GetCurrentQuantity()
    {
        return currentQuantity;
    }

    public int GetMaxQuantity()
    {
        return maxQuantity;
    }

    public int GetMinQuantity()
    {
        return minQuantity;
    }

    public bool IsQuantityConfirmed()
    {
        return quantityConfirmed;
    }

    private void OnOKButtonClick(object sender, EventArgs e)
    {
        if (ValidateAndParseQuantity())
        {
            quantityConfirmed = true;
            QuantityConfirmed?.Invoke(currentQuantity);
            Close();
        }
    }

    private void OnCancelButtonClick(object sender, EventArgs e)
    {
        quantityConfirmed = false;
        QuantityCancelled?.Invoke();
        Close();
    }

    private bool ValidateAndParseQuantity()
    {
        if (quantityInput is null)
        {
            return false;
        }
        var inputText = quantityInput.Text.Trim();
            
        if (string.IsNullOrEmpty(inputText))
        {
            ShowError("Please enter a quantity.");
            return false;
        }

        if (!int.TryParse(inputText, out var quantity))
        {
            ShowError("Please enter a valid number.");
            return false;
        }

        if (quantity < minQuantity)
        {
            ShowError($"Quantity must be at least {minQuantity}.");
            return false;
        }

        if (quantity > maxQuantity)
        {
            ShowError($"Quantity cannot exceed {maxQuantity}.");
            return false;
        }

        currentQuantity = quantity;
        return true;
    }

    private void ShowError(string message)
    {
        // Show error message (could be implemented as a message box or status text)
        // For now, we'll just set the input text to red or show a temporary message
        quantityInput.Text = "";
        quantityInput.Text = currentQuantity.ToString();
    }

    public override bool HandleEvent(Event e)
    {
        if (e is KeyEvent keyEvent && keyEvent.Type == EventType.KeyDown)
        {
            switch (keyEvent.Key)
            {
            case Silk.NET.Input.Key.Enter:
                OnOKButtonClick(this, EventArgs.Empty);
                return true;

            case Silk.NET.Input.Key.Escape:
                OnCancelButtonClick(this, EventArgs.Empty);
                return true;
            }
        }

        return base.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Update quantity input
        if (quantityInput != null)
        {
            // Check if user typed a valid number
            var inputText = quantityInput.Text.Trim();
            if (!string.IsNullOrEmpty(inputText) && int.TryParse(inputText, out var tempQuantity))
            {
                currentQuantity = tempQuantity;
            }
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);

        // Additional rendering if needed
    }

    public void SetOKButtonText(string text)
    {
        if (okButton != null)
        {
            okButton.Text = text;
        }
    }

    public void SetCancelButtonText(string text)
    {
        if (cancelButton != null)
        {
            cancelButton.Text = text;
        }
    }

    public void EnableOKButton(bool enable)
    {
        if (okButton != null)
        {
            okButton.IsEnabled = enable;
        }
    }

    public void EnableCancelButton(bool enable)
    {
        if (cancelButton != null)
        {
            cancelButton.IsEnabled = enable;
        }
    }

    public void SetInputFocus()
    {
        if (quantityInput != null)
        {
            quantityInput.SetFocus(true);
        }
    }

    public void SelectAllText()
    {
        if (quantityInput != null)
        {
            //quantityInput.SelectAll();
        }
    }

    public override void OnShown()
    {
        base.OnShown();
        SetInputFocus();
        SelectAllText();
    }

    public override void OnClosed()
    {
        base.OnClosed();
            
        // Clean up event handlers
        if (okButton != null)
        {
            okButton.Click -= OnOKButtonClick;
        }
        if (cancelButton != null)
        {
            cancelButton.Click -= OnCancelButtonClick;
        }
    }

    public static int ShowQuantityDialog(string prompt, int maxQuantity, int minQuantity = 1)
    {
        var result = -1;
        var dialogClosed = false;

        var dialog = new QuantityInputDialogPane(prompt, maxQuantity, minQuantity);
            
        dialog.QuantityConfirmed += (quantity) =>
        {
            result = quantity;
            dialogClosed = true;
        };
            
        dialog.QuantityCancelled += () =>
        {
            result = -1;
            dialogClosed = true;
        };

        dialog.Show();

        // Wait for dialog to close
        while (!dialogClosed)
        {
            // Process events
            System.Threading.Thread.Sleep(10);
        }

        return result;
    }
}