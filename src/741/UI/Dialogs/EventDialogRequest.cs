namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Event dialog request structure
/// </summary>
public class EventDialogRequest
{
    public EventInfo EventInfo { get; set; }
    public DateTime Timestamp { get; set; }
    public DialogRequestType RequestType { get; set; }
}