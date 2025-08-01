namespace DarkAges.Library.Network;

public class SkillUseMessage : NetworkMessage
{
    public string SkillName { get; set; } = "";
    public int TargetId { get; set; }

    public SkillUseMessage()
    {
        MessageType = "SkillUse";
    }
}