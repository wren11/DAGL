namespace DarkAges.Library.UI.ItemShop;

public class TextInputMenuDialog : DialogPane
{
    private TextEditControlPane _inputField;
    private string _prompt;
    private ushort _inputId;
    private TextButtonExControlPane _okButton;
    private TextButtonExControlPane _cancelButton;

    public event EventHandler<string> TextSubmitted;

    public TextInputMenuDialog(byte[] packet)
    {
        ParsePacket(packet);
        InitializeUI();
    }

    private void ParsePacket(byte[] packet)
    {
        var offset = 2;
            
        var promptLength = packet[offset++];
        _prompt = System.Text.Encoding.ASCII.GetString(packet, offset, promptLength);
        offset += promptLength;
            
        _inputId = BitConverter.ToUInt16(packet, offset);
    }

    private void InitializeUI()
    {
        Size = new System.Drawing.Size(300, 120);
        Position = new System.Drawing.Point(170, 180);

        var promptLabel = new TextEditControlPane(_prompt, new System.Drawing.Rectangle(10, 10, 280, 20), true);
        AddChild(promptLabel);

        _inputField = new TextEditControlPane("", new System.Drawing.Rectangle(10, 40, 280, 20), false);
        AddChild(_inputField);

        _okButton = new TextButtonExControlPane("OK");
        _okButton.Position = new System.Drawing.Point(10, 70);
        _okButton.Size = new System.Drawing.Size(60, 25);
        _okButton.OnClick += OnOkClicked;
        AddChild(_okButton);

        _cancelButton = new TextButtonExControlPane("Cancel");
        _cancelButton.Position = new System.Drawing.Point(80, 70);
        _cancelButton.Size = new System.Drawing.Size(60, 25);
        _cancelButton.OnClick += OnCancelClicked;
        AddChild(_cancelButton);
    }

    private void OnOkClicked(ControlPane sender)
    {
        TextSubmitted?.Invoke(this, _inputField.Text);
        Close(1);
    }

    private void OnCancelClicked(ControlPane sender)
    {
        Close(0);
    }
}