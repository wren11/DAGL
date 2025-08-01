namespace DarkAges.Library.DataStructures;

/// <summary>
/// Data structure manager statistics
/// </summary>
public struct DataStructureStatistics
{
    public int TotalAllocated;
    public int TotalFreed;
    public int CurrentUsage;
    public int PeakUsage;
    public int AllocatedChunkCount;
    public int FreeChunkCount;
    public int FreeChunkMemory;
    public int AllocationCount;
    public int DeallocationCount;
    public long AverageAllocationTime;
    public long AverageDeallocationTime;
    public MemoryPoolStatistics MemoryPoolStatistics;
}