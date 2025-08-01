namespace DarkAges.Library.UI.Terminal;

/// <summary>
/// Terminal error information
/// </summary>
public class TerminalError
{
    public TerminalErrorCode ErrorCode { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}