namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Event dialog statistics
/// </summary>
public struct EventDialogStatistics
{
    public int TotalDialogsShown;
    public int TotalDialogsAccepted;
    public int TotalDialogsDeclined;
    public int TotalDialogsClosed;
    public int ActiveDialogCount;
    public int QueueSize;
    public DateTime LastDialogTime;
}