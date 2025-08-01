using System;
using System.Collections.Generic;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

/// <summary>
/// Represents a temporary effect applied to a living entity
/// </summary>
public abstract class StatusEffect(StatusEffectType type, float duration)
{
    public StatusEffectType Type { get; protected set; } = type;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public float Duration { get; set; } = duration;
    public float RemainingTime { get; set; } = duration;
    public int StackCount { get; set; } = 1;
    public int MaxStacks { get; protected set; } = 1;
    public bool IsExpired => RemainingTime <= 0;
    public bool IsPermanent { get; protected set; }
    public WorldObject_Living? Caster { get; set; }

    public virtual void Update(float deltaTime)
    {
        if (!IsPermanent)
        {
            RemainingTime -= deltaTime;
        }
    }

    public virtual void Apply(WorldObject_Living target)
    {
        OnApply(target);
    }

    public virtual void Remove(WorldObject_Living target)
    {
        OnRemove(target);
    }

    public virtual void Stack(StatusEffect other)
    {
        if (other.Type == Type && StackCount < MaxStacks)
        {
            StackCount++;
            RemainingTime = Math.Max(RemainingTime, other.RemainingTime);
        }
    }

    protected abstract void OnApply(WorldObject_Living target);
    protected abstract void OnRemove(WorldObject_Living target);
}

// Concrete status effect implementations