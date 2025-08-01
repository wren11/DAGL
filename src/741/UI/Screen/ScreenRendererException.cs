namespace DarkAges.Library.UI.Screen;

/// <summary>
/// Screen renderer exception
/// </summary>
public class ScreenRendererException : Exception
{
    public ScreenRendererException(string message) : base(message) { }
    public ScreenRendererException(string message, Exception innerException) : base(message, innerException) { }
}