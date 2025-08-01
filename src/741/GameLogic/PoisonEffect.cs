using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class PoisonEffect : StatusEffect
{
    public int DamagePerTick { get; set; }
    public float TickInterval { get; set; } = 1.0f;
    private float _lastTick;

    public PoisonEffect(int damagePerTick, float duration) : base(StatusEffectType.Poison, duration)
    {
        Name = "Poison";
        Description = $"Takes {damagePerTick} damage every {TickInterval} seconds";
        DamagePerTick = damagePerTick;
        MaxStacks = 5;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        _lastTick += deltaTime;
    }

    protected override void OnApply(WorldObject_Living target)
    {
        // Visual effect for poison
    }

    protected override void OnRemove(WorldObject_Living target)
    {
        // Remove visual effect
    }

    public void ProcessTick(WorldObject_Living target)
    {
        if (_lastTick >= TickInterval)
        {
            target.TakeDamage(DamagePerTick * StackCount, Caster, World.DamageType.Magical);
            _lastTick = 0;
        }
    }
}