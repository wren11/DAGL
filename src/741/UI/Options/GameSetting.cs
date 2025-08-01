namespace DarkAges.Library.UI.Options;

public class GameSetting
{
    public string Name { get; set; } = string.Empty;
    public bool Value { get; set; }
    public bool DefaultValue { get; set; }
    public string Category { get; set; } = string.Empty;
}