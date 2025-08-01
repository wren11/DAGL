namespace DarkAges.Library.UI.ItemShop;

public class TextMenuDialog : DialogPane
{
    private byte _menuItemCount;
    private byte[] _menuData;
    private ushort _menuId;
    private readonly List<TextButtonExControlPane> _menuButtons = [];

    public event EventHandler<ushort> MenuItemSelected;

    public byte MenuItemCount => _menuItemCount;

    public TextMenuDialog(byte[] packet)
    {
        ParsePacket(packet);
        InitializeUI();
    }

    private void ParsePacket(byte[] packet)
    {
        var offset = 2;
            
        _menuItemCount = packet[offset++];
        _menuData = new byte[_menuItemCount * 258];
            
        for (var i = 0; i < _menuItemCount; i++)
        {
            var textLength = packet[offset++];
            Array.Copy(packet, offset, _menuData, i * 258, textLength);
            offset += textLength;
                
            _menuId = BitConverter.ToUInt16(packet, offset);
            offset += 2;
        }
    }

    private void InitializeUI()
    {
        Size = new System.Drawing.Size(300, 50 + _menuItemCount * 30);
        Position = new System.Drawing.Point(170, 115);

        for (var i = 0; i < _menuItemCount; i++)
        {
            var text = System.Text.Encoding.ASCII.GetString(_menuData, i * 258, 
                Array.IndexOf(_menuData, (byte)0, i * 258) - i * 258);
                
            var menuItem = new TextButtonExControlPane(text);
            menuItem.Position = new System.Drawing.Point(10, 10 + i * 30);
            menuItem.Size = new System.Drawing.Size(280, 25);
            menuItem.OnClick += (sender) => OnMenuItemClicked(i);
            _menuButtons.Add(menuItem);
            AddChild(menuItem);
        }
    }

    private void OnMenuItemClicked(int index)
    {
        MenuItemSelected?.Invoke(this, _menuId);
        Close(1);
    }
}