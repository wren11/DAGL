namespace DarkAges.Library.Network;

public class CharacterDataMessage : NetworkMessage
{
    public string CharacterName { get; set; } = "";
    public int Level { get; set; }
    public int Experience { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public string CurrentMap { get; set; } = "";
    public string Class { get; set; } = "";

    public CharacterDataMessage()
    {
        MessageType = "CharacterData";
    }
}