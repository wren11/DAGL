namespace DarkAges.Library.UI.SafeQuit;

public class GameState
{
    public string PlayerName { get; set; } = string.Empty;
    public int PlayerLevel { get; set; }
    public int PlayerExperience { get; set; }
    public int PlayerHealth { get; set; }
    public int PlayerMana { get; set; }
    public int PlayerGold { get; set; }
    public DateTime LastSaveTime { get; set; }

    public void SaveToFile(string filename)
    {
        try
        {
            using var writer = new System.IO.StreamWriter(filename);
            writer.WriteLine($"PlayerName={PlayerName}");
            writer.WriteLine($"PlayerLevel={PlayerLevel}");
            writer.WriteLine($"PlayerExperience={PlayerExperience}");
            writer.WriteLine($"PlayerHealth={PlayerHealth}");
            writer.WriteLine($"PlayerMana={PlayerMana}");
            writer.WriteLine($"PlayerGold={PlayerGold}");
            writer.WriteLine($"LastSaveTime={LastSaveTime:yyyy-MM-dd HH:mm:ss}");
        }
        catch
        {
        }
    }

    public void LoadFromFile(string filename)
    {
        try
        {
            if (System.IO.File.Exists(filename))
            {
                var lines = System.IO.File.ReadAllLines(filename);
                foreach (var line in lines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        switch (parts[0])
                        {
                        case "PlayerName":
                            PlayerName = parts[1];
                            break;
                        case "PlayerLevel":
                            if (int.TryParse(parts[1], out var level))
                                PlayerLevel = level;
                            break;
                        case "PlayerExperience":
                            if (int.TryParse(parts[1], out var exp))
                                PlayerExperience = exp;
                            break;
                        case "PlayerHealth":
                            if (int.TryParse(parts[1], out var health))
                                PlayerHealth = health;
                            break;
                        case "PlayerMana":
                            if (int.TryParse(parts[1], out var mana))
                                PlayerMana = mana;
                            break;
                        case "PlayerGold":
                            if (int.TryParse(parts[1], out var gold))
                                PlayerGold = gold;
                            break;
                        case "LastSaveTime":
                            if (DateTime.TryParse(parts[1], out var saveTime))
                                LastSaveTime = saveTime;
                            break;
                        }
                    }
                }
            }
        }
        catch
        {
        }
    }
}