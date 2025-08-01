using System;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using System.Drawing;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class PasswordChangeDialogPane : DialogPane
{
    private readonly TextEditControlPane _currentPasswordInput;
    private readonly TextEditControlPane _newPasswordInput;
    private readonly TextEditControlPane _confirmPasswordInput;
    private readonly ButtonControlPane _changeButton;
    private readonly ButtonControlPane _cancelButton;
    private readonly LabelControlPane _titleLabel;
    private readonly LabelControlPane _currentPasswordLabel;
    private readonly LabelControlPane _newPasswordLabel;
    private readonly LabelControlPane _confirmPasswordLabel;
    private readonly LabelControlPane _errorLabel;

    public event EventHandler<string> PasswordChanged = delegate { };
    public event EventHandler PasswordChangeCancelled = delegate { };

    public PasswordChangeDialogPane()
    {
        var font = FontManager.GetFont("default") as SimpleFont;
        if (font == null)
        {
            throw new InvalidOperationException("Default font not found");
        }

        // Create labels
        _titleLabel = new LabelControlPane(font);
        _titleLabel.Text = "Change Password";

        _currentPasswordLabel = new LabelControlPane(font);
        _currentPasswordLabel.Text = "Current Password:";

        _newPasswordLabel = new LabelControlPane(font);
        _newPasswordLabel.Text = "New Password:";

        _confirmPasswordLabel = new LabelControlPane(font);
        _confirmPasswordLabel.Text = "Confirm Password:";

        _errorLabel = new LabelControlPane(font);
        _errorLabel.TextColor = Color.Red;
        _errorLabel.Text = "";

        // Create input fields
        _currentPasswordInput = new TextEditControlPane();
        _currentPasswordInput.IsPassword = true;

        _newPasswordInput = new TextEditControlPane();
        _newPasswordInput.IsPassword = true;

        _confirmPasswordInput = new TextEditControlPane();
        _confirmPasswordInput.IsPassword = true;

        // Create buttons
        _changeButton = new ButtonControlPane();
        _changeButton.SetText("Change");
        _changeButton.Click += OnChangeButtonClick;

        _cancelButton = new ButtonControlPane();
        _cancelButton.SetText("Cancel");
        _cancelButton.Click += OnCancelButtonClick;

        // Add controls
        AddChild(_titleLabel);
        AddChild(_currentPasswordLabel);
        AddChild(_currentPasswordInput);
        AddChild(_newPasswordLabel);
        AddChild(_newPasswordInput);
        AddChild(_confirmPasswordLabel);
        AddChild(_confirmPasswordInput);
        AddChild(_errorLabel);
        AddChild(_changeButton);
        AddChild(_cancelButton);

        // Set initial layout
        SetLayout();
    }

    private void SetLayout()
    {
        var padding = 10;
        var labelWidth = 150;
        var inputWidth = 200;
        var buttonWidth = 80;
        var buttonHeight = 30;
        var lineHeight = 30;

        var startX = (GraphicsDevice.Instance.Width - (labelWidth + inputWidth + padding)) / 2;
        var startY = (GraphicsDevice.Instance.Height - (6 * lineHeight + buttonHeight)) / 2;

        // Title
        _titleLabel.Bounds = new Rectangle(startX, startY, labelWidth + inputWidth, lineHeight);
        startY += lineHeight + padding;

        // Current password
        _currentPasswordLabel.Bounds = new Rectangle(startX, startY, labelWidth, lineHeight);
        _currentPasswordInput.Bounds = new Rectangle(startX + labelWidth + padding, startY, inputWidth, lineHeight);
        startY += lineHeight + padding;

        // New password
        _newPasswordLabel.Bounds = new Rectangle(startX, startY, labelWidth, lineHeight);
        _newPasswordInput.Bounds = new Rectangle(startX + labelWidth + padding, startY, inputWidth, lineHeight);
        startY += lineHeight + padding;

        // Confirm password
        _confirmPasswordLabel.Bounds = new Rectangle(startX, startY, labelWidth, lineHeight);
        _confirmPasswordInput.Bounds = new Rectangle(startX + labelWidth + padding, startY, inputWidth, lineHeight);
        startY += lineHeight + padding;

        // Error message
        _errorLabel.Bounds = new Rectangle(startX, startY, labelWidth + inputWidth, lineHeight);
        startY += lineHeight + padding;

        // Buttons
        var buttonY = startY;
        var buttonSpacing = (labelWidth + inputWidth + padding - 2 * buttonWidth) / 3;
        _changeButton.Bounds = new Rectangle(startX + buttonSpacing, buttonY, buttonWidth, buttonHeight);
        _cancelButton.Bounds = new Rectangle(startX + buttonSpacing * 2 + buttonWidth, buttonY, buttonWidth, buttonHeight);
    }

    private void OnChangeButtonClick(object? sender, EventArgs e)
    {
        var currentPassword = _currentPasswordInput.Text;
        var newPassword = _newPasswordInput.Text;
        var confirmPassword = _confirmPasswordInput.Text;

        if (string.IsNullOrEmpty(currentPassword))
        {
            _errorLabel.Text = "Please enter your current password";
            return;
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            _errorLabel.Text = "Please enter a new password";
            return;
        }

        if (string.IsNullOrEmpty(confirmPassword))
        {
            _errorLabel.Text = "Please confirm your new password";
            return;
        }

        if (newPassword != confirmPassword)
        {
            _errorLabel.Text = "New passwords do not match";
            return;
        }

        if (newPassword.Length < 6)
        {
            _errorLabel.Text = "Password must be at least 6 characters long";
            return;
        }

        _errorLabel.Text = "";
        PasswordChanged?.Invoke(this, newPassword);
        Hide();
    }

    private void OnCancelButtonClick(object? sender, EventArgs e)
    {
        PasswordChangeCancelled?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    public override void Show()
    {
        _currentPasswordInput.Text = "";
        _newPasswordInput.Text = "";
        _confirmPasswordInput.Text = "";
        _errorLabel.Text = "";
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }
}