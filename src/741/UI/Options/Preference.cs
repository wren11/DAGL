namespace DarkAges.Library.UI.Options;

public class Preference
{
    private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
    private readonly Dictionary<string, GameSetting> _gameSettings = new Dictionary<string, GameSetting>();

    public void SetValue(string key, object value)
    {
        _values[key] = value;
    }

    public T GetValue<T>(string key, T defaultValue = default(T))
    {
        if (_values.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public void SetGameSetting(GameSetting setting)
    {
        _gameSettings[setting.Name] = setting;
    }

    public GameSetting GetGameSetting(string name)
    {
        return _gameSettings.TryGetValue(name, out var setting) ? setting : null;
    }

    public void SaveToFile(string filename)
    {
        try
        {
            using var writer = new System.IO.StreamWriter(filename);
            foreach (var kvp in _values)
            {
                writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
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
                        _values[parts[0]] = parts[1];
                    }
                }
            }
        }
        catch
        {
        }
    }
}