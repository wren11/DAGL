namespace DarkAges.Library.DataStructures;

/// <summary>
/// Tree iterator error information
/// </summary>
public class TreeIteratorError
{
    public TreeIteratorErrorCode ErrorCode { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}