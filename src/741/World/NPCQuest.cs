namespace DarkAges.Library.World;

public class NPCQuest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int RequiredLevel { get; set; } = 1;
    public bool IsCompleted { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
}