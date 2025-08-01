namespace DarkAges.Library.Common.DataStructures;

public class RingBuffer<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _tail;
    private int _size;
    private readonly int _capacity;

    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _capacity = capacity;
        _buffer = new T[capacity];
        _head = 0;
        _tail = 0;
        _size = 0;
    }

    public int Count => _size;

    public int Capacity => _capacity;

    public bool IsEmpty => _size == 0;

    public bool IsFull => _size == _capacity;

    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _head = 0;
        _tail = 0;
        _size = 0;
    }

    public void Enqueue(T item)
    {
        if (_size == _capacity)
        {
            _head = (_head + 1) % _capacity;
        }
        else
        {
            _size++;
        }

        _buffer[_tail] = item;
        _tail = (_tail + 1) % _capacity;
    }

    public T Dequeue()
    {
        if (_size == 0)
            throw new InvalidOperationException("Ring buffer is empty");

        var item = _buffer[_head];
        _buffer[_head] = default(T);
        _head = (_head + 1) % _capacity;
        _size--;

        return item;
    }

    public bool TryDequeue(out T item)
    {
        if (_size == 0)
        {
            item = default(T);
            return false;
        }

        item = Dequeue();
        return true;
    }

    public T Peek()
    {
        if (_size == 0)
            throw new InvalidOperationException("Ring buffer is empty");

        return _buffer[_head];
    }

    public bool TryPeek(out T item)
    {
        if (_size == 0)
        {
            item = default(T);
            return false;
        }

        item = _buffer[_head];
        return true;
    }

    public T[] ToArray()
    {
        var array = new T[_size];
        for (var i = 0; i < _size; i++)
        {
            array[i] = _buffer[(_head + i) % _capacity];
        }
        return array;
    }
}