namespace DarkAges.Library.UI;

/// <summary>
/// Event data for dialog events.
/// </summary>
public class DialogEventData
{
    public DialogType DialogType { get; set; }
    public bool IsSpell { get; set; }
    public int Id { get; set; }
}