namespace DarkAges.Library.Network;

public class SpellCastMessage : NetworkMessage
{
    public string SpellName { get; set; } = "";
    public int TargetId { get; set; }
    public float TargetX { get; set; }
    public float TargetY { get; set; }

    public SpellCastMessage()
    {
        MessageType = "SpellCast";
    }
}