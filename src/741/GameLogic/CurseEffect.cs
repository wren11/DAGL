using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class CurseEffect : StatusEffect
{
    public int AllStatsReduction { get; set; }

    public CurseEffect(int allStatsReduction, float duration) : base(StatusEffectType.Curse, duration)
    {
        Name = "Curse";
        Description = $"All stats reduced by {allStatsReduction}";
        AllStatsReduction = allStatsReduction;
    }

    protected override void OnApply(WorldObject_Living target)
    {
        target.Strength = Math.Max(1, target.Strength - AllStatsReduction);
        target.Intelligence = Math.Max(1, target.Intelligence - AllStatsReduction);
        target.Wisdom = Math.Max(1, target.Wisdom - AllStatsReduction);
        target.Constitution = Math.Max(1, target.Constitution - AllStatsReduction);
        target.Dexterity = Math.Max(1, target.Dexterity - AllStatsReduction);
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        target.Strength += AllStatsReduction;
        target.Intelligence += AllStatsReduction;
        target.Wisdom += AllStatsReduction;
        target.Constitution += AllStatsReduction;
        target.Dexterity += AllStatsReduction;
    }
}