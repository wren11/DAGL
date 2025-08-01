using System;
using System.Collections.Generic;

namespace DarkAges.Library.Common;

/// <summary>
/// Dynamic array implementation with automatic memory management.
/// Based on the algorithms from chunk_027.txt disassembly.
/// </summary>
/// <typeparam name="T">Type of elements stored in the array</typeparam>
public class DynamicArray<T> : IDisposable where T : struct
{
    private T[] data;
    private int count;
    private int capacity;
    private int elementSize;
    private bool isDisposed;
        
    /// <summary>
    /// Initializes a new instance of the DynamicArray.
    /// </summary>
    /// <param name="initialCapacity">Initial capacity of the array</param>
    public DynamicArray(int initialCapacity = 16)
    {
        elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
        capacity = Math.Max(1, initialCapacity);
        data = new T[capacity];
        count = 0;
        isDisposed = false;
    }
        
    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Count => count;
        
    /// <summary>
    /// Gets the current capacity of the array.
    /// </summary>
    public int Capacity => capacity;
        
    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">Index of the element</param>
    /// <returns>The element at the specified index</returns>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");
            return data[index];
        }
        set
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");
            data[index] = value;
        }
    }
        
    /// <summary>
    /// Searches for an element in the array.
    /// </summary>
    /// <param name="searchKey">The key to search for</param>
    /// <returns>Index of the found element, or -1 if not found</returns>
    public int Search(ushort searchKey)
    {
        if (isDisposed || count == 0)
            return -1;
                
        // Binary search implementation
        var left = 0;
        var right = count - 1;
            
        while (left <= right)
        {
            var mid = (left + right) / 2;
            var currentKey = GetKey(data[mid]);
                
            if (currentKey == searchKey)
                return mid;
            else if (currentKey < searchKey)
                left = mid + 1;
            else
                right = mid - 1;
        }
            
        return -1;
    }
        
    /// <summary>
    /// Adds an element to the end of the array.
    /// </summary>
    /// <param name="item">The item to add</param>
    public void Add(T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        EnsureCapacity(count + 1);
        data[count] = item;
        count++;
    }
        
    /// <summary>
    /// Inserts an element at the specified index.
    /// </summary>
    /// <param name="index">Index where to insert the element</param>
    /// <param name="item">The item to insert</param>
    public void Insert(int index, T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        if (index < 0 || index > count)
            throw new ArgumentOutOfRangeException(nameof(index));
                
        EnsureCapacity(count + 1);
            
        // Shift elements to make room
        for (var i = count; i > index; i--)
        {
            data[i] = data[i - 1];
        }
            
        data[index] = item;
        count++;
    }
        
    /// <summary>
    /// Removes the element at the specified index.
    /// </summary>
    /// <param name="index">Index of the element to remove</param>
    public void RemoveAt(int index)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index));
                
        // Shift elements to fill the gap
        for (var i = index; i < count - 1; i++)
        {
            data[i] = data[i + 1];
        }
            
        count--;
            
        // Clear the last element
        if (count >= 0)
            data[count] = default(T);
    }
        
    /// <summary>
    /// Removes all elements from the array.
    /// </summary>
    public void Clear()
    {
        if (isDisposed)
            return;
                
        Array.Clear(data, 0, count);
        count = 0;
    }
        
    /// <summary>
    /// Ensures the array has enough capacity for the specified number of elements.
    /// </summary>
    /// <param name="requiredCapacity">Required capacity</param>
    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= capacity)
            return;
                
        var newCapacity = Math.Max(capacity * 2, requiredCapacity);
            
        // Check for overflow
        if (newCapacity < 0)
            throw new OutOfMemoryException("Array capacity overflow");
                
        var newData = new T[newCapacity];
        Array.Copy(data, newData, count);
        data = newData;
        capacity = newCapacity;
    }
        
    /// <summary>
    /// Expands the array to accommodate more elements.
    /// </summary>
    /// <param name="additionalElements">Number of additional elements to accommodate</param>
    public void Expand(int additionalElements)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        if (additionalElements <= 0)
            return;
                
        EnsureCapacity(count + additionalElements);
    }
        
    /// <summary>
    /// Resizes the array to the specified size.
    /// </summary>
    /// <param name="newSize">New size of the array</param>
    public void Resize(int newSize)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        if (newSize < 0)
            throw new ArgumentOutOfRangeException(nameof(newSize));
                
        if (newSize > capacity)
        {
            EnsureCapacity(newSize);
        }
            
        // Clear elements beyond the new size
        if (newSize < count)
        {
            Array.Clear(data, newSize, count - newSize);
        }
            
        count = newSize;
    }
        
    /// <summary>
    /// Gets the key from an element for searching.
    /// </summary>
    /// <param name="item">The item to extract the key from</param>
    /// <returns>The key as a ushort</returns>
    private ushort GetKey(T item)
    {
        // This is a simplified implementation
        // In practice, this would extract a key field from the struct
        return Convert.ToUInt16(item);
    }
        
    /// <summary>
    /// Merges two arrays with conflict resolution.
    /// </summary>
    /// <param name="array1">First array</param>
    /// <param name="array2">Second array</param>
    /// <returns>Merged array</returns>
    public static DynamicArray<T> Merge(DynamicArray<T> array1, DynamicArray<T> array2)
    {
        if (array1 == null && array2 == null)
            return new DynamicArray<T>();
                
        if (array1 == null)
            return array2.Clone();
                
        if (array2 == null)
            return array1.Clone();
                
        var result = new DynamicArray<T>(array1.count + array2.count);
            
        // Copy elements from first array
        for (var i = 0; i < array1.count; i++)
        {
            result.Add(array1.data[i]);
        }
            
        // Merge elements from second array with conflict resolution
        for (var i = 0; i < array2.count; i++)
        {
            var item2 = array2.data[i];
            var key2 = result.GetKey(item2);
                
            // Check for conflicts
            var hasConflict = false;
            for (var j = 0; j < result.count; j++)
            {
                var key1 = result.GetKey(result.data[j]);
                if (key1 == key2)
                {
                    // Resolve conflict by combining elements
                    result.data[j] = ResolveConflict(result.data[j], item2);
                    hasConflict = true;
                    break;
                }
            }
                
            if (!hasConflict)
            {
                result.Add(item2);
            }
        }
            
        return result;
    }
        
    /// <summary>
    /// Resolves conflicts between two elements.
    /// </summary>
    /// <param name="item1">First item</param>
    /// <param name="item2">Second item</param>
    /// <returns>Resolved item</returns>
    private static T ResolveConflict(T item1, T item2)
    {
        // This is a simplified conflict resolution
        // In practice, this would combine fields from both items
        return item1; // Return first item as default
    }
        
    /// <summary>
    /// Creates a copy of this array.
    /// </summary>
    /// <returns>A new array with the same elements</returns>
    public DynamicArray<T> Clone()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        var clone = new DynamicArray<T>(capacity);
        Array.Copy(data, clone.data, count);
        clone.count = count;
        return clone;
    }
        
    /// <summary>
    /// Gets an enumerator for the array.
    /// </summary>
    /// <returns>An enumerator</returns>
    public IEnumerator<T> GetEnumerator()
    {
        if (isDisposed)
            yield break;
                
        for (var i = 0; i < count; i++)
        {
            yield return data[i];
        }
    }
        
    /// <summary>
    /// Converts the array to a regular array.
    /// </summary>
    /// <returns>A new array containing the elements</returns>
    public T[] ToArray()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        var result = new T[count];
        Array.Copy(data, result, count);
        return result;
    }
        
    /// <summary>
    /// Sorts the array using the default comparer.
    /// </summary>
    public void Sort()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        Array.Sort(data, 0, count);
    }
        
    /// <summary>
    /// Sorts the array using a custom comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use</param>
    public void Sort(IComparer<T> comparer)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        Array.Sort(data, 0, count, comparer);
    }
        
    /// <summary>
    /// Reverses the order of elements in the array.
    /// </summary>
    public void Reverse()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        Array.Reverse(data, 0, count);
    }
        
    /// <summary>
    /// Finds the first element that matches the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <returns>The first matching element, or default(T) if not found</returns>
    public T Find(Predicate<T> predicate)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        for (var i = 0; i < count; i++)
        {
            if (predicate(data[i]))
                return data[i];
        }
            
        return default(T);
    }
        
    /// <summary>
    /// Finds all elements that match the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <returns>An array of matching elements</returns>
    public T[] FindAll(Predicate<T> predicate)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        var result = new List<T>();
            
        for (var i = 0; i < count; i++)
        {
            if (predicate(data[i]))
                result.Add(data[i]);
        }
            
        return result.ToArray();
    }
        
    /// <summary>
    /// Removes all elements that match the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <returns>Number of elements removed</returns>
    public int RemoveAll(Predicate<T> predicate)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));
                
        var removedCount = 0;
            
        for (var i = count - 1; i >= 0; i--)
        {
            if (predicate(data[i]))
            {
                RemoveAt(i);
                removedCount++;
            }
        }
            
        return removedCount;
    }
        
    /// <summary>
    /// Disposes of the array and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
        
    /// <summary>
    /// Disposes of the array and releases resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Clear the array
                if (data != null)
                {
                    Array.Clear(data, 0, data.Length);
                    data = null;
                }
            }
                
            count = 0;
            capacity = 0;
            isDisposed = true;
        }
    }
        
    /// <summary>
    /// Finalizer for DynamicArray.
    /// </summary>
    ~DynamicArray()
    {
        Dispose(false);
    }
}