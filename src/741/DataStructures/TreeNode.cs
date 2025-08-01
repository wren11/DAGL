namespace DarkAges.Library.DataStructures;

/// <summary>
/// Represents a node in a tree structure
/// </summary>
/// <typeparam name="T">The type of value stored in the node</typeparam>
public class TreeNode<T>(T value)
{
    public T Value { get; set; } = value;
    public TreeNode<T> Left { get; set; } = null;
    public TreeNode<T> Right { get; set; } = null;
    public TreeNode<T> Parent { get; set; } = null;
    public int Height { get; set; } = 1;
    public bool IsRed { get; set; } = false; // For Red-Black trees
    public bool IsMarked { get; set; } = false; // For traversal marking

    public bool IsLeaf => Left == null && Right == null;
    public bool HasLeftChild => Left != null;
    public bool HasRightChild => Right != null;
    public bool HasChildren => Left != null || Right != null;
}