namespace DarkAges.Library.DataStructures;

/// <summary>
/// Memory pool exception
/// </summary>
public class MemoryPoolException : Exception
{
    public MemoryPoolException(string message) : base(message) { }
    public MemoryPoolException(string message, Exception innerException) : base(message, innerException) { }
}