namespace DarkAges.Library.World;

public class PlayerSpell
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public int Experience { get; set; }
    public int MaxLevel { get; set; } = 100;
    public int ManaCost { get; set; }
    public float Cooldown { get; set; }
    public float RemainingCooldown { get; set; }
    public SpellElement Element { get; set; }
    public Dictionary<string, int> Requirements { get; set; } = new();
}