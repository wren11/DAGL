using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class StunEffect : StatusEffect
{
    public StunEffect(float duration) : base(StatusEffectType.Stun, duration)
    {
        Name = "Stun";
        Description = "Cannot move, attack, or cast spells";
    }

    protected override void OnApply(WorldObject_Living target)
    {
        target.Velocity = System.Numerics.Vector2.Zero;
        target.SetState(World.WorldObjectState.Stunned);
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        if (target.State == World.WorldObjectState.Stunned)
        {
            target.SetState(World.WorldObjectState.Idle);
        }
    }
}