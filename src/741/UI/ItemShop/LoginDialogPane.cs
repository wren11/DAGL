using System.Drawing;
using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.ItemShop;

public class LoginDialogPane : DialogPane
{
    private TextEditControlPane _nameInput;
    private TextEditControlPane _passwordInput;
    private TextButtonExControlPane _okButton;
    private TextButtonExControlPane _cancelButton;

    public LoginDialogPane()
    {
        // Based on layout from _nlogin.txt in chunk 15
        Size = new Size(200, 150);
            
        _nameInput = new TextEditControlPane("", new Rectangle(50, 20, 100, 20));
        _passwordInput = new TextEditControlPane("", new Rectangle(50, 50, 100, 20));
        _passwordInput.CanFocus = true; // Assuming password field should behave like a text edit
            
        _okButton = new TextButtonExControlPane("OK");
        _okButton.Position = new Point(30, 90);
        _okButton.Size = new Size(60, 24);

        _cancelButton = new TextButtonExControlPane("Cancel");
        _cancelButton.Position = new Point(110, 90);
        _cancelButton.Size = new Size(60, 24);

        AddChild(_nameInput);
        AddChild(_passwordInput);
        AddChild(_okButton);
        AddChild(_cancelButton);

        _okButton.OnClick += OnLogin;
        _cancelButton.OnClick += OnCancel;
    }

    private void OnLogin(ControlPane sender)
    {
        // Real implementation would send a network packet with credentials
        // For now, we'll just print to console and close.
        System.Console.WriteLine($"Attempting login with Name: '{_nameInput.Text}' Password: '{_passwordInput.Text}'");
        Close(1); // 1 for OK
    }

    private void OnCancel(ControlPane sender)
    {
        Close(0); // 0 for Cancel
    }
}