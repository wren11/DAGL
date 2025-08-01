namespace DarkAges.Library.UI.ItemShop;

public class FaceMenuDialog : DialogPane
{
    private byte _faceId;
    private uint _npcId;
    private string _npcName;
    private readonly List<TextButtonExControlPane> _faceButtons = [];

    public event EventHandler<byte> FaceSelected;

    public FaceMenuDialog(byte[] packet)
    {
        ParsePacket(packet);
        InitializeUI();
    }

    private void ParsePacket(byte[] packet)
    {
        var offset = 2;
            
        _faceId = packet[offset++];
        _npcId = BitConverter.ToUInt32(packet, offset);
        offset += 4;
            
        var nameLength = BitConverter.ToUInt16(packet, offset);
        offset += 2;
        _npcName = System.Text.Encoding.ASCII.GetString(packet, offset, nameLength);
    }

    private void InitializeUI()
    {
        Size = new System.Drawing.Size(250, 150);
        Position = new System.Drawing.Point(195, 165);

        var nameLabel = new TextEditControlPane(_npcName, new System.Drawing.Rectangle(10, 10, 230, 20), true);
        AddChild(nameLabel);

        for (var i = 0; i < 4; i++)
        {
            var faceButton = new TextButtonExControlPane($"Face {i + 1}");
            faceButton.Position = new System.Drawing.Point(10 + (i % 2) * 110, 40 + (i / 2) * 30);
            faceButton.Size = new System.Drawing.Size(100, 25);
            faceButton.OnClick += (sender) => OnFaceSelected((byte)i);
            _faceButtons.Add(faceButton);
            AddChild(faceButton);
        }
    }

    private void OnFaceSelected(byte faceIndex)
    {
        FaceSelected?.Invoke(this, faceIndex);
        Close(1);
    }
}