using System.Collections;

namespace DarkAges.Library.Common.DataStructures;

public class PriorityQueue<T> : IEnumerable<T>
{
    private readonly List<T> _items;
    private readonly IComparer<T> _comparer;

    public PriorityQueue()
    {
        _items = [];
        _comparer = Comparer<T>.Default;
    }

    public PriorityQueue(IComparer<T> comparer)
    {
        _items = [];
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public int Count => _items.Count;

    public bool IsEmpty => _items.Count == 0;

    public void Clear()
    {
        _items.Clear();
    }

    public void Enqueue(T item)
    {
        _items.Add(item);
        HeapifyUp(_items.Count - 1);
    }

    public T Dequeue()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Priority queue is empty");

        var item = _items[0];
        var lastIndex = _items.Count - 1;
        _items[0] = _items[lastIndex];
        _items.RemoveAt(lastIndex);

        if (_items.Count > 0)
        {
            HeapifyDown(0);
        }

        return item;
    }

    public T Peek()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Priority queue is empty");

        return _items[0];
    }

    public bool TryDequeue(out T item)
    {
        if (_items.Count == 0)
        {
            item = default(T);
            return false;
        }

        item = Dequeue();
        return true;
    }

    public bool TryPeek(out T item)
    {
        if (_items.Count == 0)
        {
            item = default(T);
            return false;
        }

        item = _items[0];
        return true;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            var parentIndex = (index - 1) / 2;
            if (_comparer.Compare(_items[index], _items[parentIndex]) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            var leftChild = 2 * index + 1;
            var rightChild = 2 * index + 2;
            var smallest = index;

            if (leftChild < _items.Count && _comparer.Compare(_items[leftChild], _items[smallest]) < 0)
                smallest = leftChild;

            if (rightChild < _items.Count && _comparer.Compare(_items[rightChild], _items[smallest]) < 0)
                smallest = rightChild;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int index1, int index2)
    {
        var temp = _items[index1];
        _items[index1] = _items[index2];
        _items[index2] = temp;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}