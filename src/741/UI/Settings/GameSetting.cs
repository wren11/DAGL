namespace DarkAges.Library.UI.Settings;

public class GameSetting(
    string category,
    string name,
    string defaultValue,
    SettingType type,
    string[] options = null,
    int minValue = 0,
    int maxValue = 100)
{
    public string Category { get; set; } = category ?? throw new ArgumentNullException(nameof(category));
    public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Value { get; set; } = defaultValue;
    public string DefaultValue { get; set; } = defaultValue ?? throw new ArgumentNullException(nameof(defaultValue));
    public SettingType Type { get; set; } = type;
    public string[] Options { get; set; } = options;
    public int MinValue { get; set; } = minValue;
    public int MaxValue { get; set; } = maxValue;
}