using System;
using System.Collections;
using System.Collections.Generic;

namespace DarkAges.Library.Common.DataStructures;

public class Queue<T> : IEnumerable<T>
{
    private T[] _items;
    private int _head;
    private int _tail;
    private int _size;
    private int _version;

    public Queue()
    {
        _items = new T[4];
        _head = 0;
        _tail = 0;
        _size = 0;
    }

    public Queue(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _items = new T[capacity];
        _head = 0;
        _tail = 0;
        _size = 0;
    }

    public Queue(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        _items = new T[4];
        _head = 0;
        _tail = 0;
        _size = 0;

        foreach (var item in collection)
        {
            Enqueue(item);
        }
    }

    public int Count => _size;

    public bool IsEmpty => _size == 0;

    public void Clear()
    {
        if (_size > 0)
        {
            Array.Clear(_items, 0, _items.Length);
            _head = 0;
            _tail = 0;
            _size = 0;
            _version++;
        }
    }

    public bool Contains(T item)
    {
        if (_size == 0) return false;

        var index = _head;
        var count = _size;

        while (count-- > 0)
        {
            if (EqualityComparer<T>.Default.Equals(_items[index], item))
                return true;

            index = (index + 1) % _items.Length;
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        if (array.Length - arrayIndex < _size)
            throw new ArgumentException("Insufficient space in target array");

        if (_size == 0) return;

        var index = _head;
        var count = _size;

        while (count-- > 0)
        {
            array[arrayIndex++] = _items[index];
            index = (index + 1) % _items.Length;
        }
    }

    public T Dequeue()
    {
        if (_size == 0)
            throw new InvalidOperationException("Queue is empty");

        var item = _items[_head];
        _items[_head] = default(T);
        _head = (_head + 1) % _items.Length;
        _size--;
        _version++;

        return item;
    }

    public bool TryDequeue(out T item)
    {
        if (_size == 0)
        {
            item = default(T);
            return false;
        }

        item = _items[_head];
        _items[_head] = default(T);
        _head = (_head + 1) % _items.Length;
        _size--;
        _version++;

        return true;
    }

    public void Enqueue(T item)
    {
        if (_size == _items.Length)
        {
            Grow();
        }

        _items[_tail] = item;
        _tail = (_tail + 1) % _items.Length;
        _size++;
        _version++;
    }

    public T Peek()
    {
        if (_size == 0)
            throw new InvalidOperationException("Queue is empty");

        return _items[_head];
    }

    public bool TryPeek(out T item)
    {
        if (_size == 0)
        {
            item = default(T);
            return false;
        }

        item = _items[_head];
        return true;
    }

    public T[] ToArray()
    {
        var array = new T[_size];
        CopyTo(array, 0);
        return array;
    }

    public void TrimExcess()
    {
        var threshold = (int)(_items.Length * 0.9);
        if (_size < threshold)
        {
            var newItems = new T[_size];
            CopyTo(newItems, 0);
            _items = newItems;
            _head = 0;
            _tail = _size;
            _version++;
        }
    }

    private void Grow()
    {
        var newCapacity = _items.Length * 2;
        if (newCapacity < _items.Length + 4)
            newCapacity = _items.Length + 4;

        var newItems = new T[newCapacity];
        CopyTo(newItems, 0);
        _items = newItems;
        _head = 0;
        _tail = _size;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly Queue<T> _queue;
        private int _index;
        private readonly int _version;
        private T _current;

        internal Enumerator(Queue<T> queue)
        {
            _queue = queue;
            _index = -1;
            _version = queue._version;
            _current = default(T);
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_version != _queue._version)
                throw new InvalidOperationException("Collection was modified during enumeration");

            if (_index == -1)
            {
                _index = 0;
            }

            if (_index < _queue._size)
            {
                _current = _queue._items[(_queue._head + _index) % _queue._items.Length];
                _index++;
                return true;
            }

            _index = _queue._size + 1;
            _current = default(T);
            return false;
        }

        public void Reset()
        {
            if (_version != _queue._version)
                throw new InvalidOperationException("Collection was modified during enumeration");

            _index = -1;
            _current = default(T);
        }
    }
}