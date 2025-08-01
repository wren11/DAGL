using System;
using System.Collections.Generic;
using System.Threading;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// Thread-safe ring buffer implementation for efficient data buffering
/// </summary>
/// <typeparam name="T">Type of data to store in the buffer</typeparam>
public class RingBuffer<T> : IDisposable
{
    private readonly T[] buffer;
    private readonly int capacity;
    private int readIndex;
    private int writeIndex;
    private int count;
    private readonly object lockObject;
    private bool isDisposed;

    // Statistics
    private long totalReads;
    private long totalWrites;
    private long overflowCount;
    private long underflowCount;

    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));

        this.capacity = capacity;
        this.buffer = new T[capacity];
        this.readIndex = 0;
        this.writeIndex = 0;
        this.count = 0;
        this.lockObject = new object();
        this.isDisposed = false;

        // Initialize statistics
        this.totalReads = 0;
        this.totalWrites = 0;
        this.overflowCount = 0;
        this.underflowCount = 0;
    }

    /// <summary>
    /// Gets the total capacity of the buffer
    /// </summary>
    public int Capacity => capacity;

    /// <summary>
    /// Gets the current number of items in the buffer
    /// </summary>
    public int Count
    {
        get
        {
            lock (lockObject)
            {
                return count;
            }
        }
    }

    /// <summary>
    /// Gets the number of available slots for writing
    /// </summary>
    public int AvailableSpace
    {
        get
        {
            lock (lockObject)
            {
                return capacity - count;
            }
        }
    }

    /// <summary>
    /// Gets whether the buffer is empty
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            lock (lockObject)
            {
                return count == 0;
            }
        }
    }

    /// <summary>
    /// Gets whether the buffer is full
    /// </summary>
    public bool IsFull
    {
        get
        {
            lock (lockObject)
            {
                return count == capacity;
            }
        }
    }

    /// <summary>
    /// Writes a single item to the buffer
    /// </summary>
    /// <param name="item">Item to write</param>
    /// <returns>True if the item was written successfully, false if buffer is full</returns>
    public bool Write(T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        lock (lockObject)
        {
            if (count == capacity)
            {
                overflowCount++;
                return false; // Buffer is full
            }

            buffer[writeIndex] = item;
            writeIndex = (writeIndex + 1) % capacity;
            count++;
            totalWrites++;

            return true;
        }
    }

    /// <summary>
    /// Writes multiple items to the buffer
    /// </summary>
    /// <param name="items">Items to write</param>
    /// <returns>Number of items actually written</returns>
    public int Write(T[] items)
    {
        return Write(items, 0, items.Length);
    }

    /// <summary>
    /// Writes a range of items to the buffer
    /// </summary>
    /// <param name="items">Items to write</param>
    /// <param name="offset">Starting offset in the items array</param>
    /// <param name="length">Number of items to write</param>
    /// <returns>Number of items actually written</returns>
    public int Write(T[] items, int offset, int length)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (offset < 0 || length < 0 || offset + length > items.Length)
            throw new ArgumentException("Invalid offset or length");

        lock (lockObject)
        {
            var availableSpace = capacity - count;
            var itemsToWrite = Math.Min(length, availableSpace);

            if (itemsToWrite == 0)
            {
                overflowCount++;
                return 0;
            }

            for (var i = 0; i < itemsToWrite; i++)
            {
                buffer[writeIndex] = items[offset + i];
                writeIndex = (writeIndex + 1) % capacity;
            }

            count += itemsToWrite;
            totalWrites += itemsToWrite;

            return itemsToWrite;
        }
    }

    /// <summary>
    /// Reads a single item from the buffer
    /// </summary>
    /// <param name="item">Output parameter for the read item</param>
    /// <returns>True if an item was read successfully, false if buffer is empty</returns>
    public bool Read(out T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        lock (lockObject)
        {
            if (count == 0)
            {
                item = default(T);
                underflowCount++;
                return false; // Buffer is empty
            }

            item = buffer[readIndex];
            readIndex = (readIndex + 1) % capacity;
            count--;
            totalReads++;

            return true;
        }
    }

    /// <summary>
    /// Reads multiple items from the buffer
    /// </summary>
    /// <param name="items">Array to store read items</param>
    /// <returns>Number of items actually read</returns>
    public int Read(T[] items)
    {
        return Read(items, 0, items.Length);
    }

    /// <summary>
    /// Reads a range of items from the buffer
    /// </summary>
    /// <param name="items">Array to store read items</param>
    /// <param name="offset">Starting offset in the items array</param>
    /// <param name="length">Number of items to read</param>
    /// <returns>Number of items actually read</returns>
    public int Read(T[] items, int offset, int length)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (offset < 0 || length < 0 || offset + length > items.Length)
            throw new ArgumentException("Invalid offset or length");

        lock (lockObject)
        {
            var itemsToRead = Math.Min(length, count);

            if (itemsToRead == 0)
            {
                underflowCount++;
                return 0;
            }

            for (var i = 0; i < itemsToRead; i++)
            {
                items[offset + i] = buffer[readIndex];
                readIndex = (readIndex + 1) % capacity;
            }

            count -= itemsToRead;
            totalReads += itemsToRead;

            return itemsToRead;
        }
    }

    /// <summary>
    /// Peeks at the next item without removing it from the buffer
    /// </summary>
    /// <param name="item">Output parameter for the peeked item</param>
    /// <returns>True if an item was peeked successfully, false if buffer is empty</returns>
    public bool Peek(out T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        lock (lockObject)
        {
            if (count == 0)
            {
                item = default(T);
                return false;
            }

            item = buffer[readIndex];
            return true;
        }
    }

    /// <summary>
    /// Clears all items from the buffer
    /// </summary>
    public void Clear()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        lock (lockObject)
        {
            Array.Clear(buffer, 0, buffer.Length);
            readIndex = 0;
            writeIndex = 0;
            count = 0;
        }
    }

    /// <summary>
    /// Gets a copy of all items currently in the buffer
    /// </summary>
    /// <returns>Array containing all items in the buffer</returns>
    public T[] ToArray()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        lock (lockObject)
        {
            T[] result = new T[count];
                
            if (count > 0)
            {
                if (readIndex < writeIndex)
                {
                    // Data is contiguous
                    Array.Copy(buffer, readIndex, result, 0, count);
                }
                else
                {
                    // Data wraps around
                    var firstPart = capacity - readIndex;
                    Array.Copy(buffer, readIndex, result, 0, firstPart);
                    Array.Copy(buffer, 0, result, firstPart, writeIndex);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Gets statistics about the buffer usage
    /// </summary>
    /// <returns>Buffer statistics</returns>
    public RingBufferStatistics GetStatistics()
    {
        lock (lockObject)
        {
            return new RingBufferStatistics
            {
                Capacity = capacity,
                CurrentCount = count,
                AvailableSpace = capacity - count,
                TotalReads = totalReads,
                TotalWrites = totalWrites,
                OverflowCount = overflowCount,
                UnderflowCount = underflowCount,
                UtilizationPercentage = (double)count / capacity * 100.0
            };
        }
    }

    /// <summary>
    /// Resets all statistics counters
    /// </summary>
    public void ResetStatistics()
    {
        lock (lockObject)
        {
            totalReads = 0;
            totalWrites = 0;
            overflowCount = 0;
            underflowCount = 0;
        }
    }

    /// <summary>
    /// Waits for data to become available in the buffer
    /// </summary>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>True if data became available, false if timeout occurred</returns>
    public bool WaitForData(int timeout = -1)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        var startTime = DateTime.UtcNow;
            
        while (true)
        {
            lock (lockObject)
            {
                if (count > 0)
                    return true;
            }

            if (timeout >= 0)
            {
                var elapsed = DateTime.UtcNow - startTime;
                if (elapsed.TotalMilliseconds >= timeout)
                    return false;
            }

            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// Waits for space to become available in the buffer
    /// </summary>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>True if space became available, false if timeout occurred</returns>
    public bool WaitForSpace(int timeout = -1)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RingBuffer<T>));

        var startTime = DateTime.UtcNow;
            
        while (true)
        {
            lock (lockObject)
            {
                if (count < capacity)
                    return true;
            }

            if (timeout >= 0)
            {
                var elapsed = DateTime.UtcNow - startTime;
                if (elapsed.TotalMilliseconds >= timeout)
                    return false;
            }

            Thread.Sleep(1);
        }
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
                lock (lockObject)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }

            isDisposed = true;
        }
    }
}