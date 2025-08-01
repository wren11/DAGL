namespace DarkAges.Library.UI.ItemShop;

public class ServerSpellMenuDialog : DialogPane
{
    private ushort _spellCount;
    private SpellEntry[] _spells = [];
    private readonly List<TextButtonExControlPane> _spellButtons = [];

    public event EventHandler<SpellEntry> SpellSelected = delegate { };

    public ServerSpellMenuDialog(byte[] packet)
    {
        ParsePacket(packet);
        InitializeUI();
    }

    private void ParsePacket(byte[] packet)
    {
        var offset = 2;
            
        _spellCount = BitConverter.ToUInt16(packet, offset);
        offset += 2;
            
        _spells = new SpellEntry[_spellCount];
        for (var i = 0; i < _spellCount; i++)
        {
            _spells[i] = new SpellEntry
            {
                SpellId = BitConverter.ToUInt32(packet, offset),
                Level = BitConverter.ToUInt16(packet, offset + 4),
                Name = ParseNullTerminatedString(packet, offset + 7)
            };
            offset += 264;
        }
    }

    private string ParseNullTerminatedString(byte[] packet, int startOffset)
    {
        var endOffset = startOffset;
        while (endOffset < packet.Length && packet[endOffset] != 0)
        {
            endOffset++;
        }
        return System.Text.Encoding.ASCII.GetString(packet, startOffset, endOffset - startOffset);
    }

    private void InitializeUI()
    {
        Size = new System.Drawing.Size(300, 50 + _spellCount * 25);
        Position = new System.Drawing.Point(170, 115);

        for (var i = 0; i < _spellCount; i++)
        {
            var spellButton = new TextButtonExControlPane($"{_spells[i].Name} (Lv.{_spells[i].Level})");
            spellButton.Position = new System.Drawing.Point(10, 10 + i * 25);
            spellButton.Size = new System.Drawing.Size(280, 23);
            spellButton.OnClick += (sender) => OnSpellSelected(i);
            _spellButtons.Add(spellButton);
            AddChild(spellButton);
        }
    }

    private void OnSpellSelected(int index)
    {
        SpellSelected?.Invoke(this, _spells[index]);
        Close(1);
    }

    public class SpellEntry
    {
        public uint SpellId { get; set; }
        public ushort Level { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}