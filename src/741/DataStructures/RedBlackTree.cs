using System;
using System.Collections;
using System.Collections.Generic;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// A thread-safe Red-Black Tree implementation with efficient insertion, deletion, and search operations
/// </summary>
/// <typeparam name="TKey">The type of keys stored in the tree</typeparam>
/// <typeparam name="TValue">The type of values stored in the tree</typeparam>
public class RedBlackTree<TKey, TValue>(IComparer<TKey> comparer)
        : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
where TKey : IComparable<TKey>
{
    private const bool RED = false;
    private const bool BLACK = true;

    private readonly object syncLock = new object();
    private RedBlackNode root = null;
    private int count = 0;
    private bool isDisposed = false;
    private readonly IComparer<TKey> comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

    // Events
    public event Action<TKey, TValue> ItemAdded;
    public event Action<TKey, TValue> ItemRemoved;
    public event Action<TKey, TValue> ItemUpdated;
    public event Action<int> CountChanged;

    public RedBlackTree() : this(Comparer<TKey>.Default)
    {
    }

    public int Count
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));
            return count;
        }
    }

    public bool IsEmpty
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));
            return root == null;
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        lock (syncLock)
        {
            try
            {
                root = InsertNode(root, key, value);
                root.Color = BLACK;
                count++;
                CountChanged?.Invoke(count);
                ItemAdded?.Invoke(key, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add item with key {key}", ex);
            }
        }
    }

    public bool TryAdd(TKey key, TValue value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        lock (syncLock)
        {
            if (ContainsKey(key))
                return false;

            try
            {
                root = InsertNode(root, key, value);
                root.Color = BLACK;
                count++;
                CountChanged?.Invoke(count);
                ItemAdded?.Invoke(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public bool Remove(TKey key)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        lock (syncLock)
        {
            try
            {
                if (!ContainsKey(key))
                    return false;

                var value = this[key];
                root = DeleteNode(root, key);
                if (root != null)
                    root.Color = BLACK;
                count--;
                CountChanged?.Invoke(count);
                ItemRemoved?.Invoke(key, value);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove item with key {key}", ex);
            }
        }
    }

    public bool TryRemove(TKey key, out TValue value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        lock (syncLock)
        {
            value = default(TValue);

            if (!ContainsKey(key))
                return false;

            try
            {
                value = this[key];
                root = DeleteNode(root, key);
                if (root != null)
                    root.Color = BLACK;
                count--;
                CountChanged?.Invoke(count);
                ItemRemoved?.Invoke(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            lock (syncLock)
            {
                var node = FindNode(root, key);
                if (node == null)
                    throw new KeyNotFoundException($"Key {key} not found in tree");

                return node.Value;
            }
        }
        set
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            lock (syncLock)
            {
                var node = FindNode(root, key);
                if (node == null)
                {
                    Add(key, value);
                }
                else
                {
                    var oldValue = node.Value;
                    node.Value = value;
                    ItemUpdated?.Invoke(key, value);
                }
            }
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        lock (syncLock)
        {
            var node = FindNode(root, key);
            if (node == null)
            {
                value = default(TValue);
                return false;
            }

            value = node.Value;
            return true;
        }
    }

    public bool ContainsKey(TKey key)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        lock (syncLock)
        {
            return FindNode(root, key) != null;
        }
    }

    public bool ContainsValue(TValue value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        lock (syncLock)
        {
            return ContainsValueRecursive(root, value);
        }
    }

    public void Clear()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        lock (syncLock)
        {
            ClearRecursive(root);
            root = null;
            count = 0;
            CountChanged?.Invoke(count);
        }
    }

    public TKey MinKey
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

            lock (syncLock)
            {
                if (root == null)
                    throw new InvalidOperationException("Tree is empty");

                var node = FindMinNode(root);
                return node.Key;
            }
        }
    }

    public TKey MaxKey
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

            lock (syncLock)
            {
                if (root == null)
                    throw new InvalidOperationException("Tree is empty");

                var node = FindMaxNode(root);
                return node.Key;
            }
        }
    }

    public IEnumerable<TKey> Keys
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

            lock (syncLock)
            {
                var keys = new List<TKey>();
                InOrderTraversal(root, node => keys.Add(node.Key));
                return keys;
            }
        }
    }

    public IEnumerable<TValue> Values
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

            lock (syncLock)
            {
                var values = new List<TValue>();
                InOrderTraversal(root, node => values.Add(node.Value));
                return values;
            }
        }
    }

    private RedBlackNode InsertNode(RedBlackNode node, TKey key, TValue value)
    {
        if (node == null)
            return new RedBlackNode(key, value, RED);

        var comparison = comparer.Compare(key, node.Key);
        if (comparison < 0)
        {
            node.Left = InsertNode(node.Left, key, value);
        }
        else if (comparison > 0)
        {
            node.Right = InsertNode(node.Right, key, value);
        }
        else
        {
            // Key already exists, update value
            node.Value = value;
            ItemUpdated?.Invoke(key, value);
            return node;
        }

        // Fix Red-Black Tree violations
        if (IsRed(node.Right) && !IsRed(node.Left))
            node = RotateLeft(node);
        if (IsRed(node.Left) && IsRed(node.Left.Left))
            node = RotateRight(node);
        if (IsRed(node.Left) && IsRed(node.Right))
            FlipColors(node);

        return node;
    }

    private RedBlackNode DeleteNode(RedBlackNode node, TKey key)
    {
        if (node == null)
            return null;

        var comparison = comparer.Compare(key, node.Key);
        if (comparison < 0)
        {
            if (!IsRed(node.Left) && !IsRed(node.Left?.Left))
                node = MoveRedLeft(node);
            node.Left = DeleteNode(node.Left, key);
        }
        else
        {
            if (IsRed(node.Left))
                node = RotateRight(node);
            if (comparison == 0 && node.Right == null)
                return null;
            if (!IsRed(node.Right) && !IsRed(node.Right?.Left))
                node = MoveRedRight(node);
            if (comparison == 0)
            {
                var minNode = FindMinNode(node.Right);
                node.Key = minNode.Key;
                node.Value = minNode.Value;
                node.Right = DeleteMinNode(node.Right);
            }
            else
            {
                node.Right = DeleteNode(node.Right, key);
            }
        }

        return Balance(node);
    }

    private RedBlackNode FindNode(RedBlackNode node, TKey key)
    {
        while (node != null)
        {
            var comparison = comparer.Compare(key, node.Key);
            if (comparison == 0)
                return node;
            if (comparison < 0)
                node = node.Left;
            else
                node = node.Right;
        }
        return null;
    }

    private RedBlackNode FindMinNode(RedBlackNode node)
    {
        if (node == null)
            return null;

        while (node.Left != null)
            node = node.Left;

        return node;
    }

    private RedBlackNode FindMaxNode(RedBlackNode node)
    {
        if (node == null)
            return null;

        while (node.Right != null)
            node = node.Right;

        return node;
    }

    private bool ContainsValueRecursive(RedBlackNode node, TValue value)
    {
        if (node == null)
            return false;

        if (EqualityComparer<TValue>.Default.Equals(node.Value, value))
            return true;

        return ContainsValueRecursive(node.Left, value) || ContainsValueRecursive(node.Right, value);
    }

    private void ClearRecursive(RedBlackNode node)
    {
        if (node != null)
        {
            ClearRecursive(node.Left);
            ClearRecursive(node.Right);
            node.Left = null;
            node.Right = null;
        }
    }

    private void InOrderTraversal(RedBlackNode node, Action<RedBlackNode> action)
    {
        if (node != null)
        {
            InOrderTraversal(node.Left, action);
            action(node);
            InOrderTraversal(node.Right, action);
        }
    }

    // Red-Black Tree balancing operations
    private bool IsRed(RedBlackNode node)
    {
        return node != null && node.Color == RED;
    }

    private RedBlackNode RotateLeft(RedBlackNode node)
    {
        var right = node.Right;
        node.Right = right.Left;
        right.Left = node;
        right.Color = node.Color;
        node.Color = RED;
        return right;
    }

    private RedBlackNode RotateRight(RedBlackNode node)
    {
        var left = node.Left;
        node.Left = left.Right;
        left.Right = node;
        left.Color = node.Color;
        node.Color = RED;
        return left;
    }

    private void FlipColors(RedBlackNode node)
    {
        node.Color = RED;
        node.Left.Color = BLACK;
        node.Right.Color = BLACK;
    }

    private RedBlackNode MoveRedLeft(RedBlackNode node)
    {
        FlipColors(node);
        if (IsRed(node.Right?.Left))
        {
            node.Right = RotateRight(node.Right);
            node = RotateLeft(node);
            FlipColors(node);
        }
        return node;
    }

    private RedBlackNode MoveRedRight(RedBlackNode node)
    {
        FlipColors(node);
        if (IsRed(node.Left?.Left))
        {
            node = RotateRight(node);
            FlipColors(node);
        }
        return node;
    }

    private RedBlackNode Balance(RedBlackNode node)
    {
        if (IsRed(node.Right))
            node = RotateLeft(node);
        if (IsRed(node.Left) && IsRed(node.Left.Left))
            node = RotateRight(node);
        if (IsRed(node.Left) && IsRed(node.Right))
            FlipColors(node);

        return node;
    }

    private RedBlackNode DeleteMinNode(RedBlackNode node)
    {
        if (node.Left == null)
            return null;

        if (!IsRed(node.Left) && !IsRed(node.Left.Left))
            node = MoveRedLeft(node);

        node.Left = DeleteMinNode(node.Left);
        return Balance(node);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(RedBlackTree<TKey, TValue>));

        lock (syncLock)
        {
            var items = new List<KeyValuePair<TKey, TValue>>();
            InOrderTraversal(root, node => items.Add(new KeyValuePair<TKey, TValue>(node.Key, node.Value)));
            return items.GetEnumerator();
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
                    ClearRecursive(root);
                    root = null;
                }
            }

            isDisposed = true;
        }
    }

    /// <summary>
    /// Represents a node in the Red-Black Tree
    /// </summary>
    private class RedBlackNode(TKey key, TValue value, bool color)
    {
        public TKey Key { get; set; } = key;
        public TValue Value { get; set; } = value;
        public RedBlackNode Left { get; set; } = null;
        public RedBlackNode Right { get; set; } = null;
        public bool Color { get; set; } = color;
    }
}