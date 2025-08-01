namespace DarkAges.Library.UI.Macro;

public class MacroAction
{
    public MacroActionType Type { get; set; }
    public string Parameters { get; set; }
    public int Delay { get; set; } = 0;
}