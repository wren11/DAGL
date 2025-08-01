namespace DarkAges.Library.Network;

public class PlayerStatsUpdateMessage : NetworkMessage
{
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public int Experience { get; set; }
    public int Level { get; set; }

    public PlayerStatsUpdateMessage()
    {
        MessageType = "PlayerStatsUpdate";
    }
}