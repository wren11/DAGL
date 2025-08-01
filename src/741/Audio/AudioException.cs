namespace DarkAges.Library.Audio;

/// <summary>
/// Audio exception
/// </summary>
public class AudioException : Exception
{
    public AudioException(string message) : base(message) { }
    public AudioException(string message, Exception innerException) : base(message, innerException) { }
}