namespace DarkAges.Library.World;

/// <summary>
/// Event args for object collision events
/// </summary>
public class WorldObjectCollisionEventArgs(WorldObject source, WorldObject target) : EventArgs
{
    public WorldObject Source { get; } = source;
    public WorldObject Target { get; } = target;
}