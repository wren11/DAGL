namespace DarkAges.Library.UI.Region;

public class GameRegion(int id, string name, string mapName) : Region(id, name, "game")
{
    public string MapName { get; set; } = mapName ?? throw new ArgumentNullException(nameof(mapName));
    public int WeatherType { get; set; }
    public bool IsPvPEnabled { get; set; }
    public int MaxPlayers { get; set; } = 100;
    public List<string> AllowedClasses { get; set; } = [];

    public bool IsClassAllowed(string className)
    {
        return AllowedClasses.Count == 0 || AllowedClasses.Contains(className);
    }

    public void AddAllowedClass(string className)
    {
        if (!string.IsNullOrEmpty(className) && !AllowedClasses.Contains(className))
        {
            AllowedClasses.Add(className);
        }
    }

    public void RemoveAllowedClass(string className)
    {
        AllowedClasses.Remove(className);
    }
}