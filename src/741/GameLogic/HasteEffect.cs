using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class HasteEffect : StatusEffect
{
    public float SpeedMultiplier { get; set; }

    public HasteEffect(float speedMultiplier, float duration) : base(StatusEffectType.Haste, duration)
    {
        Name = "Haste";
        Description = $"Movement and attack speed increased by {(speedMultiplier - 1) * 100}%";
        SpeedMultiplier = speedMultiplier;
    }

    protected override void OnApply(WorldObject_Living target)
    {
        target.AttackSpeed *= SpeedMultiplier;
        target.CastSpeed *= SpeedMultiplier;
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        target.AttackSpeed /= SpeedMultiplier;
        target.CastSpeed /= SpeedMultiplier;
    }
}