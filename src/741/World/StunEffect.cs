using DarkAges.Library.GameLogic;

namespace DarkAges.Library.World;

public class StunEffect : StatusEffect
{
    public StunEffect(float duration) : base(StatusEffectType.Stun, duration)
    {
        Name = "Stun";
        Description = "Cannot move or act";
    }

    protected override void OnApply(WorldObject_Living target)
    {
        // Visual effect for stun
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        // Remove visual effect
    }
}