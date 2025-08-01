namespace DarkAges.Library.World;

public class HealEventArgs(WorldObject_Living target, WorldObject_Living? healer, int amount)
        : EventArgs
{
    public WorldObject_Living Target { get; } = target;
    public WorldObject_Living? Healer { get; } = healer;
    public int Amount { get; } = amount;
}