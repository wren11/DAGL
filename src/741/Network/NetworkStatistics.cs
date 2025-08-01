namespace DarkAges.Library.Network;

public class NetworkStatistics
{
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public long PacketsSent { get; set; }
    public long PacketsReceived { get; set; }
    public long ErrorCount { get; set; }
    public long RetryCount { get; set; }

    public void Reset()
    {
        BytesSent = 0;
        BytesReceived = 0;
        PacketsSent = 0;
        PacketsReceived = 0;
        ErrorCount = 0;
        RetryCount = 0;
    }
}