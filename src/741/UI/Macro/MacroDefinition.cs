namespace DarkAges.Library.UI.Macro;

public class MacroDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<MacroAction> Actions { get; set; } = [];
    public bool IsEnabled { get; set; } = true;
    public int Cooldown { get; set; } = 0;
    public DateTime LastExecuted { get; set; }
}