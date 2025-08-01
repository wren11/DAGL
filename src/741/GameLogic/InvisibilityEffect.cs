using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class InvisibilityEffect : StatusEffect
{
    public InvisibilityEffect(float duration) : base(StatusEffectType.Invisibility, duration)
    {
        Name = "Invisibility";
        Description = "Cannot be seen by enemies";
    }

    protected override void OnApply(WorldObject_Living target)
    {
        target.Opacity = 0.3f; // Semi-transparent
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        target.Opacity = 1.0f; // Fully visible
    }
}