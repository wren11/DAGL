namespace DarkAges.Library.IO;

/// <summary>
/// Serial exception
/// </summary>
public class SerialException : Exception
{
    public SerialException(string message) : base(message) { }
    public SerialException(string message, Exception innerException) : base(message, innerException) { }
}