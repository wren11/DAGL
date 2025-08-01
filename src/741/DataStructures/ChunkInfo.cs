namespace DarkAges.Library.DataStructures;

/// <summary>
/// Information about an allocated chunk
/// </summary>
public struct ChunkInfo
{
    public int Size;
    public DateTime AllocatedTime;
    public int AccessCount;
}