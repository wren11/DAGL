namespace DarkAges.Library.World;

public class DamageEventArgs(WorldObject_Living target, WorldObject_Living? attacker, int damage, DamageType damageType)
        : EventArgs
{
    public WorldObject_Living Target { get; } = target;
    public WorldObject_Living? Attacker { get; } = attacker;
    public int Damage { get; } = damage;
    public DamageType DamageType { get; } = damageType;
}