using System.Drawing;

namespace DarkAges.Library.UI.ItemShop;

public class ChangePasswordDialogPane : DialogPane
{
    private TextEditControlPane _nameInput;
    private TextEditControlPane _passwordInput;
    private TextEditControlPane _newPasswordInput;
    private TextEditControlPane _confirmInput;
    private TextButtonExControlPane _okButton;
    private TextButtonExControlPane _cancelButton;

    public ChangePasswordDialogPane()
    {
        Size = new Size(240, 200);

        _nameInput = new TextEditControlPane("", new Rectangle(100, 20, 120, 20));
        _passwordInput = new TextEditControlPane("", new Rectangle(100, 50, 120, 20));
        _newPasswordInput = new TextEditControlPane("", new Rectangle(100, 80, 120, 20));
        _confirmInput = new TextEditControlPane("", new Rectangle(100, 110, 120, 20));
            
        _okButton = new TextButtonExControlPane("OK");
        _okButton.Position = new Point(50, 150);
        _okButton.Size = new Size(60, 24);

        _cancelButton = new TextButtonExControlPane("Cancel");
        _cancelButton.Position = new Point(130, 150);
        _cancelButton.Size = new Size(60, 24);
            
        AddChild(new Label("Name:", new Point(20, 22)));
        AddChild(new Label("Password:", new Point(20, 52)));
        AddChild(new Label("New Password:", new Point(20, 82)));
        AddChild(new Label("Confirm:", new Point(20, 112)));
        AddChild(_nameInput);
        AddChild(_passwordInput);
        AddChild(_newPasswordInput);
        AddChild(_confirmInput);
        AddChild(_okButton);
        AddChild(_cancelButton);
    }
}