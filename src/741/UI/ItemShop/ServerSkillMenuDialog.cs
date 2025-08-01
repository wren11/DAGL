namespace DarkAges.Library.UI.ItemShop;

public class ServerSkillMenuDialog : DialogPane
{
    private ushort _skillCount;
    private SkillEntry[] _skills = [];
    private readonly List<TextButtonExControlPane> _skillButtons = [];

    public event EventHandler<SkillEntry> SkillSelected = delegate { };

    public ServerSkillMenuDialog(byte[] packet)
    {
        ParsePacket(packet);
        InitializeUI();
    }

    private void ParsePacket(byte[] packet)
    {
        var offset = 2;
            
        _skillCount = BitConverter.ToUInt16(packet, offset);
        offset += 2;
            
        _skills = new SkillEntry[_skillCount];
        for (var i = 0; i < _skillCount; i++)
        {
            _skills[i] = new SkillEntry
            {
                SkillId = BitConverter.ToUInt32(packet, offset),
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
        Size = new System.Drawing.Size(300, 50 + _skillCount * 25);
        Position = new System.Drawing.Point(170, 115);

        for (var i = 0; i < _skillCount; i++)
        {
            var skillButton = new TextButtonExControlPane($"{_skills[i].Name} (Lv.{_skills[i].Level})");
            skillButton.Position = new System.Drawing.Point(10, 10 + i * 25);
            skillButton.Size = new System.Drawing.Size(280, 23);
            skillButton.OnClick += (sender) => OnSkillSelected(i);
            _skillButtons.Add(skillButton);
            AddChild(skillButton);
        }
    }

    private void OnSkillSelected(int index)
    {
        SkillSelected?.Invoke(this, _skills[index]);
        Close(1);
    }

    public class SkillEntry
    {
        public uint SkillId { get; set; }
        public ushort Level { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}