namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Event error information
/// </summary>
public class EventError
{
    public EventErrorCode ErrorCode { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
}