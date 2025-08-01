namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Event message structure
/// </summary>
public class EventMessage
{
    public byte[]? Data { get; set; }
    public DateTime Timestamp { get; set; }
    public int SourceId { get; set; }
    public EventMessageType Type { get; set; }
}