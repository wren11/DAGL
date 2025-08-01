namespace DarkAges.Library.UI.Terminal;

/// <summary>
/// Terminal statistics
/// </summary>
public struct TerminalStatistics
{
    public int LineCount;
    public int CursorX;
    public int CursorY;
    public bool IsConnected;
    public int TerminalMode;
    public DateTime LastActivity;
    public int CommandHistoryCount;
}