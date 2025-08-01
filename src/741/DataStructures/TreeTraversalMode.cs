namespace DarkAges.Library.DataStructures;

/// <summary>
/// Tree traversal modes
/// </summary>
public enum TreeTraversalMode
{
    /// <summary>
    /// In-order traversal: Left -> Root -> Right
    /// </summary>
    InOrder,

    /// <summary>
    /// Pre-order traversal: Root -> Left -> Right
    /// </summary>
    PreOrder,

    /// <summary>
    /// Post-order traversal: Left -> Right -> Root
    /// </summary>
    PostOrder,

    /// <summary>
    /// Level-order traversal (breadth-first)
    /// </summary>
    LevelOrder
}