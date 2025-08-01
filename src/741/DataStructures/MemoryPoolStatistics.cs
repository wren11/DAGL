namespace DarkAges.Library.DataStructures;

/// <summary>
/// Memory pool statistics
/// </summary>
public struct MemoryPoolStatistics
{
    public int TotalAllocated;
    public int TotalFreed;
    public int CurrentUsage;
    public int PeakUsage;
    public int PoolCount;
    public int PooledMemory;
    public int AllocationCount;
    public int DeallocationCount;
    public long AverageAllocationTime;
    public long AverageDeallocationTime;
}