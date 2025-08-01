namespace DarkAges.Library.UI;

/// <summary>
/// Data for generic window change events.
/// </summary>
public class GenericWindowData
{
    public int EventType { get; set; }
    public IntPtr EventData { get; set; }
    public int EventSize { get; set; }
}