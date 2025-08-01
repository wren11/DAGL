namespace DarkAges.Library.DataStructures;

/// <summary>
/// Statistics about ring buffer usage
/// </summary>
public struct RingBufferStatistics
{
    public int Capacity;
    public int CurrentCount;
    public int AvailableSpace;
    public long TotalReads;
    public long TotalWrites;
    public long OverflowCount;
    public long UnderflowCount;
    public double UtilizationPercentage;
}