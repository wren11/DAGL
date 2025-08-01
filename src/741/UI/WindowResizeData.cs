namespace DarkAges.Library.UI;

/// <summary>
/// Data for window resize events.
/// </summary>
public class WindowResizeData
{
    public int NewWidth { get; set; }
    public int NewHeight { get; set; }
    public int OldWidth { get; set; }
    public int OldHeight { get; set; }
}