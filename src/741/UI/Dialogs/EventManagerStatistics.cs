namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Event manager statistics
/// </summary>
public struct EventManagerStatistics
{
    public int TotalEventsProcessed;
    public int TotalEventsAccepted;
    public int TotalEventsDeclined;
    public int TotalEventsFailed;
    public int ActiveEventCount;
    public int TotalEventCount;
    public int QueueSize;
    public DateTime LastEventTime;
}