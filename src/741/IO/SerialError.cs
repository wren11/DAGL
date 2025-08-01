namespace DarkAges.Library.IO;

/// <summary>
/// Serial error information
/// </summary>
public class SerialError
{
    public SerialErrorCode ErrorCode { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}