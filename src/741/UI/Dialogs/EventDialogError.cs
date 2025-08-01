namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Event dialog error information
/// </summary>
public class EventDialogError
{
    public EventDialogErrorCode ErrorCode { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
}