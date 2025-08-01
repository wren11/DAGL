using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class RegenerationEffect : StatusEffect
{
    public int HealPerTick { get; set; }
    public float TickInterval { get; set; } = 2.0f;
    private float _lastTick;

    public RegenerationEffect(int healPerTick, float duration) : base(StatusEffectType.Regeneration, duration)
    {
        Name = "Regeneration";
        Description = $"Restores {healPerTick} health every {TickInterval} seconds";
        HealPerTick = healPerTick;
        MaxStacks = 3;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        _lastTick += deltaTime;
    }

    protected override void OnApply(WorldObject_Living target)
    {
        // Visual effect for regeneration
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        // Remove visual effect
    }

    public void ProcessTick(WorldObject_Living target)
    {
        if (_lastTick >= TickInterval)
        {
            target.Heal(HealPerTick * StackCount);
            _lastTick = 0;
        }
    }
}