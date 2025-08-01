namespace DarkAges.Library.Graphics;

/// <summary>
/// World background image error information
/// </summary>
public class WorldBackImageError
{
    public WorldBackImageErrorCode ErrorCode { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}