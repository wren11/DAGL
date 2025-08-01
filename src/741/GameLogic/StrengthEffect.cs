using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class StrengthEffect : StatusEffect
{
    public int StrengthBonus { get; set; }

    public StrengthEffect(int strengthBonus, float duration) : base(StatusEffectType.Strength, duration)
    {
        Name = "Strength";
        Description = $"Strength increased by {strengthBonus}";
        StrengthBonus = strengthBonus;
        MaxStacks = 3;
    }

    protected override void OnApply(WorldObject_Living target)
    {
        target.Strength += StrengthBonus;
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        target.Strength -= StrengthBonus * StackCount;
    }
}