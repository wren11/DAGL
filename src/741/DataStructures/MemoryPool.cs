using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// Manages memory allocation and pooling for efficient memory usage
/// </summary>
public class MemoryPool : IDisposable
{
    private const int DEFAULT_POOL_SIZE = 1024;
    private const int MAX_POOL_SIZE = 65536;
    private const int ALIGNMENT = 8;

    private readonly object syncLock = new object();
    private Dictionary<int, Queue<IntPtr>> pools;
    private Dictionary<IntPtr, int> allocatedSizes;
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
    public event Action<int> MemoryAllocated;
    public event Action<int> MemoryFreed;
    public event Action<MemoryPoolException> PoolError;

    public MemoryPool()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        pools = new Dictionary<int, Queue<IntPtr>>();
        allocatedSizes = new Dictionary<IntPtr, int>();
        isDisposed = false;
        totalAllocated = 0;
        totalFreed = 0;
        peakUsage = 0;
        totalAllocationTime = 0;
        totalDeallocationTime = 0;
        allocationCount = 0;
        deallocationCount = 0;
    }

    public IntPtr Allocate(int size)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(MemoryPool));

        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive");

        lock (syncLock)
        {
            try
            {
                var startTime = DateTime.Now.Ticks;
                    
                // Align size to 8-byte boundary
                var alignedSize = AlignSize(size);
                    
                // Try to get from pool first
                var ptr = GetFromPool(alignedSize);
                    
                if (ptr == IntPtr.Zero)
                {
                    // Allocate new memory
                    ptr = Marshal.AllocHGlobal(alignedSize);
                    if (ptr == IntPtr.Zero)
                    {
                        throw new MemoryPoolException("Failed to allocate memory");
                    }
                }

                // Track allocation
                allocatedSizes[ptr] = alignedSize;
                totalAllocated += alignedSize;
                allocationCount++;
                    
                // Update peak usage
                var currentUsage = totalAllocated - totalFreed;
                if (currentUsage > peakUsage)
                {
                    peakUsage = currentUsage;
                }

                // Clear memory
                ClearMemory(ptr, alignedSize);

                var endTime = DateTime.Now.Ticks;
                totalAllocationTime += endTime - startTime;

                MemoryAllocated?.Invoke(alignedSize);
                return ptr;
            }
            catch (Exception ex)
            {
                PoolError?.Invoke(new MemoryPoolException("Allocation failed", ex));
                throw;
            }
        }
    }

    public IntPtr AllocateAligned(int size, int alignment)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(MemoryPool));

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
                    
                // Allocate with extra space for alignment
                var totalSize = alignedSize + alignment - 1;
                var ptr = Marshal.AllocHGlobal(totalSize);
                    
                if (ptr == IntPtr.Zero)
                {
                    throw new MemoryPoolException("Failed to allocate aligned memory");
                }

                // Align the pointer
                var alignedPtr = AlignPointer(ptr, alignment);
                    
                // Track allocation
                allocatedSizes[alignedPtr] = alignedSize;
                totalAllocated += alignedSize;
                allocationCount++;
                    
                // Update peak usage
                var currentUsage = totalAllocated - totalFreed;
                if (currentUsage > peakUsage)
                {
                    peakUsage = currentUsage;
                }

                // Clear memory
                ClearMemory(alignedPtr, alignedSize);

                var endTime = DateTime.Now.Ticks;
                totalAllocationTime += endTime - startTime;

                MemoryAllocated?.Invoke(alignedSize);
                return alignedPtr;
            }
            catch (Exception ex)
            {
                PoolError?.Invoke(new MemoryPoolException("Aligned allocation failed", ex));
                throw;
            }
        }
    }

    public void Free(IntPtr ptr)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(MemoryPool));

        if (ptr == IntPtr.Zero)
            return;

        lock (syncLock)
        {
            try
            {
                var startTime = DateTime.Now.Ticks;

                if (allocatedSizes.TryGetValue(ptr, out var size))
                {
                    // Return to pool if pool size allows
                    if (ShouldReturnToPool(size))
                    {
                        ReturnToPool(ptr, size);
                    }
                    else
                    {
                        // Free memory directly
                        Marshal.FreeHGlobal(ptr);
                    }

                    allocatedSizes.Remove(ptr);
                    totalFreed += size;
                    deallocationCount++;

                    var endTime = DateTime.Now.Ticks;
                    totalDeallocationTime += endTime - startTime;

                    MemoryFreed?.Invoke(size);
                }
                else
                {
                    // Not tracked, free directly
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch (Exception ex)
            {
                PoolError?.Invoke(new MemoryPoolException("Deallocation failed", ex));
                throw;
            }
        }
    }

    public void Reallocate(IntPtr ptr, int newSize)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(MemoryPool));

        if (newSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(newSize), "Size must be positive");

        lock (syncLock)
        {
            try
            {
                if (allocatedSizes.TryGetValue(ptr, out var oldSize))
                {
                    var alignedNewSize = AlignSize(newSize);
                        
                    if (alignedNewSize <= oldSize)
                    {
                        // Shrink allocation
                        allocatedSizes[ptr] = alignedNewSize;
                        totalAllocated -= (oldSize - alignedNewSize);
                    }
                    else
                    {
                        // Grow allocation
                        var newPtr = Allocate(alignedNewSize);
                            
                        // Copy data
                        if (oldSize > 0)
                        {
                            CopyMemory(newPtr, ptr, oldSize);
                        }
                            
                        // Free old allocation
                        Free(ptr);
                            
                        // Update tracking
                        allocatedSizes[newPtr] = alignedNewSize;
                        ptr = newPtr;
                    }
                }
                else
                {
                    throw new MemoryPoolException("Pointer not found in allocation tracking");
                }
            }
            catch (Exception ex)
            {
                PoolError?.Invoke(new MemoryPoolException("Reallocation failed", ex));
                throw;
            }
        }
    }

    private IntPtr GetFromPool(int size)
    {
        if (pools.TryGetValue(size, out var pool) && pool.Count > 0)
        {
            return pool.Dequeue();
        }
        return IntPtr.Zero;
    }

    private void ReturnToPool(IntPtr ptr, int size)
    {
        if (!pools.ContainsKey(size))
        {
            pools[size] = new Queue<IntPtr>();
        }

        var pool = pools[size];
        if (pool.Count < MAX_POOL_SIZE)
        {
            pool.Enqueue(ptr);
        }
        else
        {
            // Pool is full, free memory directly
            Marshal.FreeHGlobal(ptr);
        }
    }

    private bool ShouldReturnToPool(int size)
    {
        return size <= DEFAULT_POOL_SIZE;
    }

    private int AlignSize(int size)
    {
        return AlignSize(size, ALIGNMENT);
    }

    private int AlignSize(int size, int alignment)
    {
        return (size + alignment - 1) & ~(alignment - 1);
    }

    private IntPtr AlignPointer(IntPtr ptr, int alignment)
    {
        var address = ptr.ToInt64();
        var alignedAddress = (address + alignment - 1) & ~(alignment - 1);
        return new IntPtr(alignedAddress);
    }

    private void ClearMemory(IntPtr ptr, int size)
    {
        // Clear memory to zero
        var zeroBuffer = new byte[Math.Min(size, 1024)];
        for (var offset = 0; offset < size; offset += zeroBuffer.Length)
        {
            var copySize = Math.Min(zeroBuffer.Length, size - offset);
            Marshal.Copy(zeroBuffer, 0, IntPtr.Add(ptr, offset), copySize);
        }
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

    public void ClearPool()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(MemoryPool));

        lock (syncLock)
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var ptr = pool.Dequeue();
                    Marshal.FreeHGlobal(ptr);
                }
            }
            pools.Clear();
        }
    }

    public void TrimPool()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(MemoryPool));

        lock (syncLock)
        {
            foreach (var kvp in pools)
            {
                var pool = kvp.Value;
                var keepCount = Math.Min(pool.Count / 2, DEFAULT_POOL_SIZE);
                    
                while (pool.Count > keepCount)
                {
                    var ptr = pool.Dequeue();
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
    }

    public MemoryPoolStatistics GetStatistics()
    {
        lock (syncLock)
        {
            var poolCount = 0;
            var pooledMemory = 0;
                
            foreach (var kvp in pools)
            {
                poolCount += kvp.Value.Count;
                pooledMemory += kvp.Key * kvp.Value.Count;
            }

            return new MemoryPoolStatistics
            {
                TotalAllocated = totalAllocated,
                TotalFreed = totalFreed,
                CurrentUsage = totalAllocated - totalFreed,
                PeakUsage = peakUsage,
                PoolCount = poolCount,
                PooledMemory = pooledMemory,
                AllocationCount = allocationCount,
                DeallocationCount = deallocationCount,
                AverageAllocationTime = allocationCount > 0 ? totalAllocationTime / allocationCount : 0,
                AverageDeallocationTime = deallocationCount > 0 ? totalDeallocationTime / deallocationCount : 0
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
        }
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
                    // Free all pooled memory
                    ClearPool();
                        
                    // Free any remaining allocated memory
                    foreach (var kvp in allocatedSizes)
                    {
                        Marshal.FreeHGlobal(kvp.Key);
                    }
                    allocatedSizes.Clear();
                        
                    pools.Clear();
                }
            }

            isDisposed = true;
        }
    }
}