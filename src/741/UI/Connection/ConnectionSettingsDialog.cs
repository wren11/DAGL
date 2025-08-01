using System.Drawing;

namespace DarkAges.Library.UI.Connection;

/// <summary>
/// Settings dialog for connection configuration
/// </summary>
public class ConnectionSettingsDialog : DialogPane
{
    private CheckBoxControlPane encryptionCheckBox;
    private TextEditControlPane timeoutInput;
    private TextEditControlPane retryInput;
    private TextButtonExControlPane okButton;
    private TextButtonExControlPane cancelButton;

    public ConnectionSettingsDialog()
    {
        Size = new Size(300, 200);
        Title = "Connection Settings";

        encryptionCheckBox = new CheckBoxControlPane("Use Encryption");
        encryptionCheckBox.Position = new Point(20, 40);

        timeoutInput = new TextEditControlPane("20000", new Rectangle(20, 70, 100, 25));
        timeoutInput.Label = "Timeout (ms):";

        retryInput = new TextEditControlPane("5", new Rectangle(20, 100, 100, 25));
        retryInput.Label = "Retry Count:";

        okButton = new TextButtonExControlPane("OK");
        okButton.Position = new Point(150, 150);
        okButton.Size = new Size(60, 25);
        okButton.OnClick += OnOkClicked;

        cancelButton = new TextButtonExControlPane("Cancel");
        cancelButton.Position = new Point(220, 150);
        cancelButton.Size = new Size(60, 25);
        cancelButton.OnClick += OnCancelClicked;

        AddChild(encryptionCheckBox);
        AddChild(timeoutInput);
        AddChild(retryInput);
        AddChild(okButton);
        AddChild(cancelButton);
    }

    private void OnOkClicked(ControlPane sender)
    {
        //Close(1);
    }

    private void OnCancelClicked(ControlPane sender)
    {
        //Close(0);
    }
}