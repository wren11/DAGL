using System;
using System.Collections;
using System.Collections.Generic;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// A dynamic array implementation with automatic resizing and memory management
/// </summary>
/// <typeparam name="T">The type of elements stored in the array</typeparam>
public class DynamicArray<T> : IEnumerable<T>, IDisposable
{
    private const int DEFAULT_CAPACITY = 16;
    private const int GROWTH_FACTOR = 2;
    private const int MAX_CAPACITY = 0x7FEFFFFF;

    private T[] items;
    private int count;
    private int capacity;
    private bool isDisposed;
    private readonly object syncLock = new object();

    // Events
    public event Action<int> CapacityChanged;
    public event Action<int> CountChanged;
    public event Action<T> ItemAdded;
    public event Action<T> ItemRemoved;
    public event Action<int, T> ItemSet;

    public DynamicArray() : this(DEFAULT_CAPACITY)
    {
    }

    public DynamicArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity cannot be negative");

        capacity = Math.Max(initialCapacity, DEFAULT_CAPACITY);
        items = new T[capacity];
        count = 0;
        isDisposed = false;
    }

    public DynamicArray(IEnumerable<T> collection) : this()
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        foreach (var item in collection)
        {
            Add(item);
        }
    }

    public int Count
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DynamicArray<T>));
            return count;
        }
    }

    public int Capacity
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DynamicArray<T>));
            return capacity;
        }
        set
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DynamicArray<T>));

            if (value < count)
                throw new ArgumentOutOfRangeException(nameof(value), "Capacity cannot be less than current count");

            if (value > MAX_CAPACITY)
                throw new ArgumentOutOfRangeException(nameof(value), "Capacity exceeds maximum allowed value");

            if (value != capacity)
            {
                SetCapacity(value);
            }
        }
    }

    public T this[int index]
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DynamicArray<T>));

            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return items[index];
        }
        set
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DynamicArray<T>));

            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index));

            items[index] = value;
            ItemSet?.Invoke(index, value);
        }
    }

    public void Add(T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        lock (syncLock)
        {
            if (count == capacity)
            {
                EnsureCapacity(count + 1);
            }

            items[count] = item;
            count++;
                
            CountChanged?.Invoke(count);
            ItemAdded?.Invoke(item);
        }
    }

    public void AddRange(IEnumerable<T> collection)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        lock (syncLock)
        {
            if (collection is ICollection<T> coll)
            {
                var requiredCapacity = count + coll.Count;
                if (requiredCapacity > capacity)
                {
                    EnsureCapacity(requiredCapacity);
                }

                coll.CopyTo(items, count);
                count += coll.Count;
            }
            else
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }
        }
    }

    public void Insert(int index, T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (index < 0 || index > count)
            throw new ArgumentOutOfRangeException(nameof(index));

        lock (syncLock)
        {
            if (count == capacity)
            {
                EnsureCapacity(count + 1);
            }

            if (index < count)
            {
                Array.Copy(items, index, items, index + 1, count - index);
            }

            items[index] = item;
            count++;
                
            CountChanged?.Invoke(count);
            ItemAdded?.Invoke(item);
        }
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (index < 0 || index > count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        lock (syncLock)
        {
            if (collection is ICollection<T> coll)
            {
                var requiredCapacity = count + coll.Count;
                if (requiredCapacity > capacity)
                {
                    EnsureCapacity(requiredCapacity);
                }

                if (index < count)
                {
                    Array.Copy(items, index, items, index + coll.Count, count - index);
                }

                coll.CopyTo(items, index);
                count += coll.Count;
            }
            else
            {
                foreach (var item in collection)
                {
                    Insert(index++, item);
                }
            }
        }
    }

    public bool Remove(T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        lock (syncLock)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
    }

    public void RemoveAt(int index)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index));

        lock (syncLock)
        {
            var removedItem = items[index];
            count--;

            if (index < count)
            {
                Array.Copy(items, index + 1, items, index, count - index);
            }

            items[count] = default(T);
                
            CountChanged?.Invoke(count);
            ItemRemoved?.Invoke(removedItem);
        }
    }

    public void RemoveRange(int index, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (index < 0 || count < 0 || index + count > this.count)
            throw new ArgumentOutOfRangeException();

        lock (syncLock)
        {
            if (count > 0)
            {
                this.count -= count;
                if (index < this.count)
                {
                    Array.Copy(items, index + count, items, index, this.count - index);
                }

                Array.Clear(items, this.count, count);
                CountChanged?.Invoke(this.count);
            }
        }
    }

    public void Clear()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        lock (syncLock)
        {
            if (count > 0)
            {
                Array.Clear(items, 0, count);
                count = 0;
                CountChanged?.Invoke(count);
            }
        }
    }

    public bool Contains(T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        return IndexOf(item) >= 0;
    }

    public int IndexOf(T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        return Array.IndexOf(items, item, 0, count);
    }

    public int IndexOf(T item, int startIndex)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (startIndex < 0 || startIndex > count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        return Array.IndexOf(items, item, startIndex, count - startIndex);
    }

    public int IndexOf(T item, int startIndex, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (startIndex < 0 || count < 0 || startIndex + count > this.count)
            throw new ArgumentOutOfRangeException();

        return Array.IndexOf(items, item, startIndex, count);
    }

    public int LastIndexOf(T item)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        return Array.LastIndexOf(items, item, count - 1, count);
    }

    public int LastIndexOf(T item, int startIndex)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (startIndex < 0 || startIndex >= count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        return Array.LastIndexOf(items, item, startIndex, startIndex + 1);
    }

    public int LastIndexOf(T item, int startIndex, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (startIndex < 0 || count < 0 || startIndex - count + 1 < 0)
            throw new ArgumentOutOfRangeException();

        return Array.LastIndexOf(items, item, startIndex, count);
    }

    public void Reverse()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        lock (syncLock)
        {
            Array.Reverse(items, 0, count);
        }
    }

    public void Reverse(int index, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (index < 0 || count < 0 || index + count > this.count)
            throw new ArgumentOutOfRangeException();

        lock (syncLock)
        {
            Array.Reverse(items, index, count);
        }
    }

    public void Sort()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        lock (syncLock)
        {
            Array.Sort(items, 0, count);
        }
    }

    public void Sort(IComparer<T> comparer)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        lock (syncLock)
        {
            Array.Sort(items, 0, count, comparer);
        }
    }

    public void Sort(int index, int count, IComparer<T> comparer)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (index < 0 || count < 0 || index + count > this.count)
            throw new ArgumentOutOfRangeException();

        lock (syncLock)
        {
            Array.Sort(items, index, count, comparer);
        }
    }

    public T[] ToArray()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        T[] array = new T[count];
        Array.Copy(items, array, count);
        return array;
    }

    public void CopyTo(T[] array)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (array == null)
            throw new ArgumentNullException(nameof(array));

        CopyTo(array, 0);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < count)
            throw new ArgumentException("Destination array is not large enough");

        Array.Copy(items, 0, array, arrayIndex, count);
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (index < 0 || count < 0 || index + count > this.count)
            throw new ArgumentOutOfRangeException();

        if (arrayIndex < 0 || arrayIndex + count > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        Array.Copy(items, index, array, arrayIndex, count);
    }

    public void TrimExcess()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        lock (syncLock)
        {
            var threshold = (int)(capacity * 0.9);
            if (count < threshold)
            {
                SetCapacity(count);
            }
        }
    }

    private void EnsureCapacity(int minCapacity)
    {
        if (capacity < minCapacity)
        {
            var newCapacity = capacity == 0 ? DEFAULT_CAPACITY : capacity * GROWTH_FACTOR;
            if (newCapacity < minCapacity)
                newCapacity = minCapacity;
            if (newCapacity > MAX_CAPACITY)
                newCapacity = MAX_CAPACITY;

            SetCapacity(newCapacity);
        }
    }

    private void SetCapacity(int newCapacity)
    {
        if (newCapacity != capacity)
        {
            T[] newItems = new T[newCapacity];
            if (count > 0)
            {
                Array.Copy(items, newItems, count);
            }
            items = newItems;
            capacity = newCapacity;
            CapacityChanged?.Invoke(capacity);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DynamicArray<T>));

        for (var i = 0; i < count; i++)
        {
            yield return items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
                    Array.Clear(items, 0, count);
                    items = null;
                    count = 0;
                    capacity = 0;
                }
            }

            isDisposed = true;
        }
    }
}