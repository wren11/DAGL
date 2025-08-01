namespace DarkAges.Library.Common.DataStructures;

public class RedBlackTreeNode<TKey, TValue>(TKey key, TValue value)
{
    public TKey Key { get; set; } = key;
    public TValue Value { get; set; } = value;
    public NodeColor Color { get; set; } = NodeColor.Red;
    public RedBlackTreeNode<TKey, TValue> Left { get; set; }
    public RedBlackTreeNode<TKey, TValue> Right { get; set; }
    public RedBlackTreeNode<TKey, TValue> Parent { get; set; }
}