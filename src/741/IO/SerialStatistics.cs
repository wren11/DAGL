namespace DarkAges.Library.IO;

/// <summary>
/// Serial statistics
/// </summary>
public struct SerialStatistics
{
    public long BytesReceived;
    public long BytesSent;
    public int ConnectionAttempts;
    public DateTime LastActivity;
    public bool IsConnected;
    public int PortNumber;
    public int BaudRate;
}