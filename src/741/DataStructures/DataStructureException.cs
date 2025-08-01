namespace DarkAges.Library.DataStructures;

/// <summary>
/// Data structure exception
/// </summary>
public class DataStructureException : Exception
{
    public DataStructureException(string message) : base(message) { }
    public DataStructureException(string message, Exception innerException) : base(message, innerException) { }
}