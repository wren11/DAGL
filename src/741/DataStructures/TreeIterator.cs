using System;
using System.Collections;
using System.Collections.Generic;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// Iterator for traversing tree data structures with various traversal orders
/// </summary>
/// <typeparam name="T">The type of elements stored in the tree</typeparam>
public class TreeIterator<T> : IEnumerator<T>, IDisposable
{
    private readonly object syncLock = new object();
    private TreeNode<T> root;
    private TreeNode<T> current;
    private Stack<TreeNode<T>> nodeStack;
    private Queue<TreeNode<T>> nodeQueue;
    private TreeTraversalMode traversalMode;
    private bool isDisposed;
    private bool isInitialized;

    // Events
    public event Action<T> NodeVisited;
    public event Action<TreeNode<T>> NodeTraversed;
    public event Action<TreeIteratorError> IteratorError;

    public TreeIterator(TreeNode<T> root, TreeTraversalMode mode = TreeTraversalMode.InOrder)
    {
        this.root = root;
        this.traversalMode = mode;
        isDisposed = false;
        isInitialized = false;
            
        InitializeIterator();
    }

    private void InitializeIterator()
    {
        try
        {
            switch (traversalMode)
            {
            case TreeTraversalMode.InOrder:
            case TreeTraversalMode.PreOrder:
            case TreeTraversalMode.PostOrder:
                nodeStack = new Stack<TreeNode<T>>();
                break;
            case TreeTraversalMode.LevelOrder:
                nodeQueue = new Queue<TreeNode<T>>();
                break;
            }

            current = null;
            isInitialized = true;
        }
        catch (Exception ex)
        {
            IteratorError?.Invoke(new TreeIteratorError
            {
                ErrorCode = TreeIteratorErrorCode.InitializationFailed,
                Message = $"Failed to initialize iterator: {ex.Message}",
                Exception = ex
            });
            throw;
        }
    }

    public T Current
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(TreeIterator<T>));

            if (current == null)
                throw new InvalidOperationException("Iterator is not positioned on a valid element");

            return current.Value;
        }
    }

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        if (!isInitialized)
            InitializeIterator();

        try
        {
            switch (traversalMode)
            {
            case TreeTraversalMode.InOrder:
                return MoveNextInOrder();
            case TreeTraversalMode.PreOrder:
                return MoveNextPreOrder();
            case TreeTraversalMode.PostOrder:
                return MoveNextPostOrder();
            case TreeTraversalMode.LevelOrder:
                return MoveNextLevelOrder();
            default:
                return false;
            }
        }
        catch (Exception ex)
        {
            IteratorError?.Invoke(new TreeIteratorError
            {
                ErrorCode = TreeIteratorErrorCode.TraversalFailed,
                Message = $"Failed to move to next element: {ex.Message}",
                Exception = ex
            });
            return false;
        }
    }

    private bool MoveNextInOrder()
    {
        while (nodeStack.Count > 0 || current != null)
        {
            if (current != null)
            {
                nodeStack.Push(current);
                current = current.Left;
            }
            else
            {
                current = nodeStack.Pop();
                var result = current;
                current = current.Right;
                    
                NodeVisited?.Invoke(result.Value);
                NodeTraversed?.Invoke(result);
                return true;
            }
        }
        return false;
    }

    private bool MoveNextPreOrder()
    {
        if (current == null && root != null)
        {
            current = root;
            NodeVisited?.Invoke(current.Value);
            NodeTraversed?.Invoke(current);
            return true;
        }

        if (nodeStack.Count > 0)
        {
            current = nodeStack.Pop();
                
            if (current.Right != null)
                nodeStack.Push(current.Right);
            if (current.Left != null)
                nodeStack.Push(current.Left);

            NodeVisited?.Invoke(current.Value);
            NodeTraversed?.Invoke(current);
            return true;
        }

        return false;
    }

    private bool MoveNextPostOrder()
    {
        TreeNode<T> lastVisited = null;

        while (nodeStack.Count > 0 || current != null)
        {
            if (current != null)
            {
                nodeStack.Push(current);
                current = current.Left;
            }
            else
            {
                var peekNode = nodeStack.Peek();
                    
                if (peekNode.Right != null && peekNode.Right != lastVisited)
                {
                    current = peekNode.Right;
                }
                else
                {
                    current = nodeStack.Pop();
                    lastVisited = current;
                        
                    NodeVisited?.Invoke(current.Value);
                    NodeTraversed?.Invoke(current);
                    return true;
                }
            }
        }
        return false;
    }

    private bool MoveNextLevelOrder()
    {
        if (current == null && root != null)
        {
            nodeQueue.Enqueue(root);
        }

        if (nodeQueue.Count > 0)
        {
            current = nodeQueue.Dequeue();
                
            if (current.Left != null)
                nodeQueue.Enqueue(current.Left);
            if (current.Right != null)
                nodeQueue.Enqueue(current.Right);

            NodeVisited?.Invoke(current.Value);
            NodeTraversed?.Invoke(current);
            return true;
        }

        return false;
    }

    public void Reset()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        try
        {
            current = null;
            isInitialized = false;
                
            nodeStack?.Clear();
            nodeQueue?.Clear();
                
            InitializeIterator();
        }
        catch (Exception ex)
        {
            IteratorError?.Invoke(new TreeIteratorError
            {
                ErrorCode = TreeIteratorErrorCode.ResetFailed,
                Message = $"Failed to reset iterator: {ex.Message}",
                Exception = ex
            });
            throw;
        }
    }

    public void SetTraversalMode(TreeTraversalMode mode)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        if (traversalMode != mode)
        {
            traversalMode = mode;
            Reset();
        }
    }

    public TreeTraversalMode GetTraversalMode()
    {
        return traversalMode;
    }

    public TreeNode<T> GetCurrentNode()
    {
        return current;
    }

    public TreeNode<T> GetRoot()
    {
        return root;
    }

    public void SetRoot(TreeNode<T> newRoot)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        root = newRoot;
        Reset();
    }

    public List<T> ToList()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        var result = new List<T>();
        Reset();

        while (MoveNext())
        {
            result.Add(Current);
        }

        return result;
    }

    public T[] ToArray()
    {
        return ToList().ToArray();
    }

    public void ForEach(Action<T> action)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        if (action == null)
            throw new ArgumentNullException(nameof(action));

        Reset();
        while (MoveNext())
        {
            action(Current);
        }
    }

    public bool Any(Func<T, bool> predicate)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        Reset();
        while (MoveNext())
        {
            if (predicate(Current))
                return true;
        }
        return false;
    }

    public T FirstOrDefault(Func<T, bool> predicate = null)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        Reset();
        while (MoveNext())
        {
            if (predicate == null || predicate(Current))
                return Current;
        }
        return default(T);
    }

    public int Count(Func<T, bool> predicate = null)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(TreeIterator<T>));

        var count = 0;
        Reset();
        while (MoveNext())
        {
            if (predicate == null || predicate(Current))
                count++;
        }
        return count;
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
                nodeStack?.Clear();
                nodeQueue?.Clear();
                current = null;
                root = null;
            }

            isDisposed = true;
        }
    }
}