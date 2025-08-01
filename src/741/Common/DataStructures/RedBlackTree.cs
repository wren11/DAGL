using System;
using System.Collections;
using System.Collections.Generic;

namespace DarkAges.Library.Common.DataStructures;

public class RedBlackTree<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    private RedBlackTreeNode<TKey, TValue>? _root;
    private readonly IComparer<TKey> _comparer;
    private int _count;

    public RedBlackTree(IComparer<TKey>? comparer = null)
    {
        _comparer = comparer ?? Comparer<TKey>.Default;
        _root = null;
        _count = 0;
    }

    public int Count => _count;

    public void Insert(TKey key, TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        var newNode = new RedBlackTreeNode<TKey, TValue>(key, value);
        if (_root == null)
        {
            _root = newNode;
            _root.Color = NodeColor.Black;
            _count++;
            return;
        }

        RedBlackTreeNode<TKey, TValue>? current = _root, parent = null;
        while (current != null)
        {
            parent = current;
            var comparison = _comparer.Compare(key, current.Key);
            if (comparison < 0)
                current = current.Left;
            else if (comparison > 0)
                current = current.Right;
            else
            {
                current.Value = value; 
                return;
            }
        }

        newNode.Parent = parent;
        if (_comparer.Compare(key, parent!.Key) < 0)
            parent.Left = newNode;
        else
            parent.Right = newNode;
            
        _count++;
        FixupInsert(newNode);
    }

    /// <summary>
    /// Adds a key-value pair to the tree. This is an alias for Insert method.
    /// </summary>
    /// <param name="key">The key to add</param>
    /// <param name="value">The value to add</param>
    public void Add(TKey key, TValue value)
    {
        Insert(key, value);
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        var node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    private RedBlackTreeNode<TKey, TValue>? FindNode(TKey key)
    {
        var current = _root;
        while (current != null)
        {
            var comparison = _comparer.Compare(key, current.Key);
            if (comparison == 0) return current;
            current = comparison < 0 ? current.Left : current.Right;
        }
        return null;
    }

    private void FixupInsert(RedBlackTreeNode<TKey, TValue> node)
    {
        while (node != _root && node.Parent?.Color == NodeColor.Red)
        {
            if (node.Parent == node.Parent.Parent?.Left)
            {
                var uncle = node.Parent.Parent.Right;
                if (uncle?.Color == NodeColor.Red)
                {
                    node.Parent.Color = NodeColor.Black;
                    uncle.Color = NodeColor.Black;
                    node.Parent.Parent.Color = NodeColor.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent.Right)
                    {
                        node = node.Parent;
                        RotateLeft(node);
                    }
                    if (node.Parent != null)
                    {
                        node.Parent.Color = NodeColor.Black;
                        if (node.Parent.Parent != null)
                        {
                            node.Parent.Parent.Color = NodeColor.Red;
                            RotateRight(node.Parent.Parent);
                        }
                    }
                }
            }
            else
            {
                var uncle = node.Parent?.Parent?.Left;
                if (uncle?.Color == NodeColor.Red)
                {
                    node.Parent!.Color = NodeColor.Black;
                    uncle.Color = NodeColor.Black;
                    node.Parent.Parent!.Color = NodeColor.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent?.Left)
                    {
                        node = node.Parent;
                        RotateRight(node);
                    }
                    if (node.Parent != null)
                    {
                        node.Parent.Color = NodeColor.Black;
                        if (node.Parent.Parent != null)
                        {
                            node.Parent.Parent.Color = NodeColor.Red;
                            RotateLeft(node.Parent.Parent);
                        }
                    }
                }
            }
        }
        if (_root != null)
            _root.Color = NodeColor.Black;
    }

    private void RotateLeft(RedBlackTreeNode<TKey, TValue> node)
    {
        var pivot = node.Right;
        if (pivot == null) return;

        node.Right = pivot.Left;
        if (pivot.Left != null) pivot.Left.Parent = node;
        pivot.Parent = node.Parent;
        if (node.Parent == null) _root = pivot;
        else if (node == node.Parent.Left) node.Parent.Left = pivot;
        else node.Parent.Right = pivot;
        pivot.Left = node;
        node.Parent = pivot;
    }
        
    private void RotateRight(RedBlackTreeNode<TKey, TValue> node)
    {
        var pivot = node.Left;
        if (pivot == null) return;

        node.Left = pivot.Right;
        if (pivot.Right != null) pivot.Right.Parent = node;
        pivot.Parent = node.Parent;
        if (node.Parent == null) _root = pivot;
        else if (node == node.Parent.Right) node.Parent.Right = pivot;
        else node.Parent.Left = pivot;
        pivot.Right = node;
        node.Parent = pivot;
    }

    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        var node = FindNode(key);
        if (node == null) return false;

        DeleteNode(node);
        _count--;
        return true;
    }

    private void DeleteNode(RedBlackTreeNode<TKey, TValue> node)
    {
        var y = node;
        var yOriginalColor = y.Color;
        RedBlackTreeNode<TKey, TValue>? x;

        if (node.Left == null)
        {
            x = node.Right;
            Transplant(node, node.Right);
        }
        else if (node.Right == null)
        {
            x = node.Left;
            Transplant(node, node.Left);
        }
        else
        {
            y = Minimum(node.Right);
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == node)
            {
                if (x != null) x.Parent = y;
            }
            else
            {
                Transplant(y, y.Right);
                y.Right = node.Right;
                if (y.Right != null) y.Right.Parent = y;
            }
            Transplant(node, y);
            y.Left = node.Left;
            if (y.Left != null) y.Left.Parent = y;
            y.Color = node.Color;
        }

        if (yOriginalColor == NodeColor.Black && x != null)
        {
            DeleteFixup(x);
        }
    }

    private void Transplant(RedBlackTreeNode<TKey, TValue> u, RedBlackTreeNode<TKey, TValue>? v)
    {
        if (u.Parent == null)
            _root = v;
        else if (u == u.Parent.Left)
            u.Parent.Left = v;
        else
            u.Parent.Right = v;
        if (v != null)
            v.Parent = u.Parent;
    }

    private RedBlackTreeNode<TKey, TValue> Minimum(RedBlackTreeNode<TKey, TValue> node)
    {
        while (node.Left != null)
            node = node.Left;
        return node;
    }

    private void DeleteFixup(RedBlackTreeNode<TKey, TValue> x)
    {
        while (x != _root && x.Color == NodeColor.Black)
        {
            if (x.Parent == null) break;

            if (x == x.Parent.Left)
            {
                var w = x.Parent.Right;
                if (w == null) break;

                if (w.Color == NodeColor.Red)
                {
                    w.Color = NodeColor.Black;
                    x.Parent.Color = NodeColor.Red;
                    RotateLeft(x.Parent);
                    w = x.Parent.Right;
                }

                if (w == null) break;

                if ((w.Left == null || w.Left.Color == NodeColor.Black) &&
                    (w.Right == null || w.Right.Color == NodeColor.Black))
                {
                    w.Color = NodeColor.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Right == null || w.Right.Color == NodeColor.Black)
                    {
                        if (w.Left != null)
                            w.Left.Color = NodeColor.Black;
                        w.Color = NodeColor.Red;
                        RotateRight(w);
                        w = x.Parent.Right;
                    }

                    if (w != null)
                    {
                        w.Color = x.Parent.Color;
                        x.Parent.Color = NodeColor.Black;
                        if (w.Right != null)
                            w.Right.Color = NodeColor.Black;
                        RotateLeft(x.Parent);
                        x = _root!;
                    }
                }
            }
            else
            {
                var w = x.Parent.Left;
                if (w == null) break;

                if (w.Color == NodeColor.Red)
                {
                    w.Color = NodeColor.Black;
                    x.Parent.Color = NodeColor.Red;
                    RotateRight(x.Parent);
                    w = x.Parent.Left;
                }

                if (w == null) break;

                if ((w.Right == null || w.Right.Color == NodeColor.Black) &&
                    (w.Left == null || w.Left.Color == NodeColor.Black))
                {
                    w.Color = NodeColor.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Left == null || w.Left.Color == NodeColor.Black)
                    {
                        if (w.Right != null)
                            w.Right.Color = NodeColor.Black;
                        w.Color = NodeColor.Red;
                        RotateLeft(w);
                        w = x.Parent.Left;
                    }

                    if (w != null)
                    {
                        w.Color = x.Parent.Color;
                        x.Parent.Color = NodeColor.Black;
                        if (w.Left != null)
                            w.Left.Color = NodeColor.Black;
                        RotateRight(x.Parent);
                        x = _root!;
                    }
                }
            }
        }
        x.Color = NodeColor.Black;
    }

    public void Clear()
    {
        _root = null;
        _count = 0;
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return FindNode(key) != null;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new RedBlackTreeEnumerator<TKey, TValue>(_root);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class RedBlackTreeEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
{
    private readonly Stack<RedBlackTreeNode<TKey, TValue>> _stack;
    private RedBlackTreeNode<TKey, TValue>? _current;
    private readonly RedBlackTreeNode<TKey, TValue>? _root;

    public RedBlackTreeEnumerator(RedBlackTreeNode<TKey, TValue>? root)
    {
        _root = root;
        _stack = new Stack<RedBlackTreeNode<TKey, TValue>>();
        _current = null;
        Reset();
    }

    public KeyValuePair<TKey, TValue> Current => 
        _current == null ? 
            throw new InvalidOperationException("Enumeration has not started or has already finished.") : 
            new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (_stack.Count == 0) return false;

        _current = _stack.Pop();
        var node = _current.Right;
        while (node != null)
        {
            _stack.Push(node);
            node = node.Left;
        }

        return true;
    }

    public void Reset()
    {
        _stack.Clear();
        var node = _root;
        while (node != null)
        {
            _stack.Push(node);
            node = node.Left;
        }
    }

    public void Dispose()
    {
        // No unmanaged resources to dispose
    }
}