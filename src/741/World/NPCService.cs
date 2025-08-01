namespace DarkAges.Library.World;

public class NPCService
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Cost { get; set; } = 0;
    public NPCServiceType Type { get; set; }
}