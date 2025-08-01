using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class BlessingEffect : StatusEffect
{
    public int AllStatsBonus { get; set; }

    public BlessingEffect(int allStatsBonus, float duration) : base(StatusEffectType.Blessing, duration)
    {
        Name = "Blessing";
        Description = $"All stats increased by {allStatsBonus}";
        AllStatsBonus = allStatsBonus;
    }

    protected override void OnApply(WorldObject_Living target)
    {
        target.Strength += AllStatsBonus;
        target.Intelligence += AllStatsBonus;
        target.Wisdom += AllStatsBonus;
        target.Constitution += AllStatsBonus;
        target.Dexterity += AllStatsBonus;
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        target.Strength -= AllStatsBonus;
        target.Intelligence -= AllStatsBonus;
        target.Wisdom -= AllStatsBonus;
        target.Constitution -= AllStatsBonus;
        target.Dexterity -= AllStatsBonus;
    }
}