using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// Manages data structures and memory allocation for the Dark Ages engine
/// </summary>
public class DataStructureManager : IDisposable
{
    private const int DEFAULT_CHUNK_SIZE = 1024;
    private const int MAX_CHUNK_SIZE = 65536;
    private const int ALIGNMENT = 8;

    private readonly object syncLock = new object();
    private Dictionary<int, Queue<IntPtr>> freeChunks;
    private Dictionary<IntPtr, ChunkInfo> allocatedChunks;
    private MemoryPool memoryPool;
    private bool isDisposed;
    private int totalAllocated;
    private int totalFreed;
    private int peakUsage;

    // Statistics
    private long totalAllocationTime;
    private long totalDeallocationTime;
    private int allocationCount;
    private int deallocationCount;

    // Events
    public event Action<int> ChunkAllocated;
    public event Action<int> ChunkFreed;
    public event Action<DataStructureException> ManagerError;

    public DataStructureManager()
    {
        InitializeManager();
    }

    private void InitializeManager()
    {
        freeChunks = new Dictionary<int, Queue<IntPtr>>();
        allocatedChunks = new Dictionary<IntPtr, ChunkInfo>();
        memoryPool = new MemoryPool();
        isDisposed = false;
        totalAllocated = 0;
        totalFreed = 0;
        peakUsage = 0;
        totalAllocationTime = 0;
        totalDeallocationTime = 0;
        allocationCount = 0;
        deallocationCount = 0;

        // Subscribe to memory pool events
        memoryPool.MemoryAllocated += OnMemoryAllocated;
        memoryPool.MemoryFreed += OnMemoryFreed;
        memoryPool.PoolError += OnPoolError;
    }

    public IntPtr AllocateChunk(int size)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive");

        lock (syncLock)
        {
            try
            {
                var startTime = DateTime.Now.Ticks;
                    
                // Align size
                var alignedSize = AlignSize(size);
                    
                // Try to get from free chunks first
                var chunk = GetFromFreeChunks(alignedSize);
                    
                if (chunk == IntPtr.Zero)
                {
                    // Allocate new chunk
                    chunk = memoryPool.Allocate(alignedSize);
                }

                // Track allocation
                allocatedChunks[chunk] = new ChunkInfo
                {
                    Size = alignedSize,
                    AllocatedTime = DateTime.Now,
                    AccessCount = 0
                };

                totalAllocated += alignedSize;
                allocationCount++;
                    
                // Update peak usage
                var currentUsage = totalAllocated - totalFreed;
                if (currentUsage > peakUsage)
                {
                    peakUsage = currentUsage;
                }

                var endTime = DateTime.Now.Ticks;
                totalAllocationTime += endTime - startTime;

                ChunkAllocated?.Invoke(alignedSize);
                return chunk;
            }
            catch (Exception ex)
            {
                ManagerError?.Invoke(new DataStructureException("Chunk allocation failed", ex));
                throw;
            }
        }
    }

    public IntPtr AllocateChunkAligned(int size, int alignment)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive");

        if (alignment <= 0 || (alignment & (alignment - 1)) != 0)
            throw new ArgumentException("Alignment must be a power of 2", nameof(alignment));

        lock (syncLock)
        {
            try
            {
                var startTime = DateTime.Now.Ticks;
                    
                // Calculate aligned size
                var alignedSize = AlignSize(size, alignment);
                    
                // Allocate aligned chunk
                var chunk = memoryPool.AllocateAligned(alignedSize, alignment);
                    
                // Track allocation
                allocatedChunks[chunk] = new ChunkInfo
                {
                    Size = alignedSize,
                    AllocatedTime = DateTime.Now,
                    AccessCount = 0
                };

                totalAllocated += alignedSize;
                allocationCount++;
                    
                // Update peak usage
                var currentUsage = totalAllocated - totalFreed;
                if (currentUsage > peakUsage)
                {
                    peakUsage = currentUsage;
                }

                var endTime = DateTime.Now.Ticks;
                totalAllocationTime += endTime - startTime;

                ChunkAllocated?.Invoke(alignedSize);
                return chunk;
            }
            catch (Exception ex)
            {
                ManagerError?.Invoke(new DataStructureException("Aligned chunk allocation failed", ex));
                throw;
            }
        }
    }

    public void FreeChunk(IntPtr chunk)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        if (chunk == IntPtr.Zero)
            return;

        lock (syncLock)
        {
            try
            {
                var startTime = DateTime.Now.Ticks;

                if (allocatedChunks.TryGetValue(chunk, out var info))
                {
                    // Return to free chunks if appropriate
                    if (ShouldReturnToFreeChunks(info.Size))
                    {
                        ReturnToFreeChunks(chunk, info.Size);
                    }
                    else
                    {
                        // Free memory directly
                        memoryPool.Free(chunk);
                    }

                    allocatedChunks.Remove(chunk);
                    totalFreed += info.Size;
                    deallocationCount++;

                    var endTime = DateTime.Now.Ticks;
                    totalDeallocationTime += endTime - startTime;

                    ChunkFreed?.Invoke(info.Size);
                }
                else
                {
                    // Not tracked, free directly
                    memoryPool.Free(chunk);
                }
            }
            catch (Exception ex)
            {
                ManagerError?.Invoke(new DataStructureException("Chunk deallocation failed", ex));
                throw;
            }
        }
    }

    public void ReallocateChunk(IntPtr chunk, int newSize)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        if (newSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(newSize), "Size must be positive");

        lock (syncLock)
        {
            try
            {
                if (allocatedChunks.TryGetValue(chunk, out var info))
                {
                    var alignedNewSize = AlignSize(newSize);
                        
                    if (alignedNewSize <= info.Size)
                    {
                        // Shrink allocation
                        allocatedChunks[chunk] = new ChunkInfo
                        {
                            Size = alignedNewSize,
                            AllocatedTime = info.AllocatedTime,
                            AccessCount = info.AccessCount
                        };
                        totalAllocated -= (info.Size - alignedNewSize);
                    }
                    else
                    {
                        // Grow allocation
                        var newChunk = AllocateChunk(alignedNewSize);
                            
                        // Copy data
                        if (info.Size > 0)
                        {
                            CopyMemory(newChunk, chunk, info.Size);
                        }
                            
                        // Free old allocation
                        FreeChunk(chunk);
                            
                        // Update tracking
                        allocatedChunks[newChunk] = new ChunkInfo
                        {
                            Size = alignedNewSize,
                            AllocatedTime = info.AllocatedTime,
                            AccessCount = info.AccessCount
                        };
                        chunk = newChunk;
                    }
                }
                else
                {
                    throw new DataStructureException("Chunk not found in allocation tracking");
                }
            }
            catch (Exception ex)
            {
                ManagerError?.Invoke(new DataStructureException("Chunk reallocation failed", ex));
                throw;
            }
        }
    }

    public void AccessChunk(IntPtr chunk)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        if (chunk == IntPtr.Zero)
            return;

        lock (syncLock)
        {
            if (allocatedChunks.TryGetValue(chunk, out var info))
            {
                info.AccessCount++;
                allocatedChunks[chunk] = info;
            }
        }
    }

    public ChunkInfo GetChunkInfo(IntPtr chunk)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        if (chunk == IntPtr.Zero)
            return default;

        lock (syncLock)
        {
            return allocatedChunks.TryGetValue(chunk, out var info) ? info : default;
        }
    }

    private IntPtr GetFromFreeChunks(int size)
    {
        if (freeChunks.TryGetValue(size, out var queue) && queue.Count > 0)
        {
            return queue.Dequeue();
        }
        return IntPtr.Zero;
    }

    private void ReturnToFreeChunks(IntPtr chunk, int size)
    {
        if (!freeChunks.ContainsKey(size))
        {
            freeChunks[size] = new Queue<IntPtr>();
        }

        var queue = freeChunks[size];
        if (queue.Count < MAX_CHUNK_SIZE)
        {
            queue.Enqueue(chunk);
        }
        else
        {
            // Queue is full, free memory directly
            memoryPool.Free(chunk);
        }
    }

    private bool ShouldReturnToFreeChunks(int size)
    {
        return size <= DEFAULT_CHUNK_SIZE;
    }

    private int AlignSize(int size)
    {
        return AlignSize(size, ALIGNMENT);
    }

    private int AlignSize(int size, int alignment)
    {
        return (size + alignment - 1) & ~(alignment - 1);
    }

    private void CopyMemory(IntPtr destination, IntPtr source, int size)
    {
        // Copy memory from source to destination
        var buffer = new byte[Math.Min(size, 1024)];
        for (var offset = 0; offset < size; offset += buffer.Length)
        {
            var copySize = Math.Min(buffer.Length, size - offset);
            Marshal.Copy(IntPtr.Add(source, offset), buffer, 0, copySize);
            Marshal.Copy(buffer, 0, IntPtr.Add(destination, offset), copySize);
        }
    }

    public void ClearFreeChunks()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        lock (syncLock)
        {
            foreach (var queue in freeChunks.Values)
            {
                while (queue.Count > 0)
                {
                    var chunk = queue.Dequeue();
                    memoryPool.Free(chunk);
                }
            }
            freeChunks.Clear();
        }
    }

    public void TrimFreeChunks()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataStructureManager));

        lock (syncLock)
        {
            foreach (var kvp in freeChunks)
            {
                var queue = kvp.Value;
                var keepCount = Math.Min(queue.Count / 2, DEFAULT_CHUNK_SIZE);
                    
                while (queue.Count > keepCount)
                {
                    var chunk = queue.Dequeue();
                    memoryPool.Free(chunk);
                }
            }
        }
    }

    public DataStructureStatistics GetStatistics()
    {
        lock (syncLock)
        {
            var freeChunkCount = 0;
            var freeChunkMemory = 0;
                
            foreach (var kvp in freeChunks)
            {
                freeChunkCount += kvp.Value.Count;
                freeChunkMemory += kvp.Key * kvp.Value.Count;
            }

            var memoryStats = memoryPool.GetStatistics();

            return new DataStructureStatistics
            {
                TotalAllocated = totalAllocated,
                TotalFreed = totalFreed,
                CurrentUsage = totalAllocated - totalFreed,
                PeakUsage = peakUsage,
                AllocatedChunkCount = allocatedChunks.Count,
                FreeChunkCount = freeChunkCount,
                FreeChunkMemory = freeChunkMemory,
                AllocationCount = allocationCount,
                DeallocationCount = deallocationCount,
                AverageAllocationTime = allocationCount > 0 ? totalAllocationTime / allocationCount : 0,
                AverageDeallocationTime = deallocationCount > 0 ? totalDeallocationTime / deallocationCount : 0,
                MemoryPoolStatistics = memoryStats
            };
        }
    }

    public void ResetStatistics()
    {
        lock (syncLock)
        {
            totalAllocated = 0;
            totalFreed = 0;
            peakUsage = 0;
            totalAllocationTime = 0;
            totalDeallocationTime = 0;
            allocationCount = 0;
            deallocationCount = 0;
            memoryPool.ResetStatistics();
        }
    }

    private void OnMemoryAllocated(int size)
    {
        // Handle memory pool allocation events
    }

    private void OnMemoryFreed(int size)
    {
        // Handle memory pool deallocation events
    }

    private void OnPoolError(MemoryPoolException ex)
    {
        ManagerError?.Invoke(new DataStructureException("Memory pool error", ex));
    }

    public bool IsDisposed()
    {
        return isDisposed;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                lock (syncLock)
                {
                    // Clear free chunks
                    ClearFreeChunks();
                        
                    // Free all allocated chunks
                    foreach (var kvp in allocatedChunks)
                    {
                        memoryPool.Free(kvp.Key);
                    }
                    allocatedChunks.Clear();
                        
                    // Dispose memory pool
                    memoryPool?.Dispose();
                        
                    freeChunks.Clear();
                }
            }

            isDisposed = true;
        }
    }
}