namespace DarkAges.Library.UI.ItemShop;

public class ArgumentedTextMenuDialog : TextMenuDialog
{
    private string _argument;

    public ArgumentedTextMenuDialog(byte[] packet) : base(packet)
    {
        var offset = 2 + 1 + MenuItemCount * 258;
        var argLength = packet[offset++];
        _argument = System.Text.Encoding.ASCII.GetString(packet, offset, argLength);
    }

    public string Argument => _argument;
}