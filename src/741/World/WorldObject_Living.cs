using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using DarkAges.Library.GameLogic;

namespace DarkAges.Library.World;

/// <summary>
/// Base class for all living entities in the world (humans, monsters, NPCs)
/// </summary>
public abstract class WorldObject_Living(WorldObjectType objectType) : WorldObject(objectType)
{
    // Core stats
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Mana { get; set; } = 100;
    public int MaxMana { get; set; } = 100;

    // Primary attributes  
    public int Strength { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Dexterity { get; set; } = 10;

    // Derived stats
    public int ArmorClass { get; set; } = 0;
    public int MagicResistance { get; set; } = 0;
    public int Damage { get; set; } = 0;
    public int Hit { get; set; } = 0;

    // Status effects
    public readonly HashSet<StatusEffect> StatusEffects = [];
        
    // Combat state
    public WorldObject_Living? Target { get; set; }
    public DateTime LastAttackTime { get; set; }
    public DateTime LastSpellTime { get; set; }
    public float AttackSpeed { get; set; } = 1.0f;
    public float CastSpeed { get; set; } = 1.0f;

    // Animation
    public short AnimationFrame { get; set; }
    public short EmotionFrame { get; set; }
    public DateTime LastAnimationUpdate { get; set; } = DateTime.Now;

    // Events
    public event EventHandler<StatusEffectEventArgs>? StatusEffectAdded;
    public event EventHandler<StatusEffectEventArgs>? StatusEffectRemoved;
    public event EventHandler<DamageEventArgs>? DamageTaken;
    public event EventHandler<HealEventArgs>? HealthRestored;
    public event EventHandler<WorldObject_Living>? TargetChanged;
    public event EventHandler<WorldObject_Living?>? Died;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!IsActive || IsDisposed) return;

        // Update status effects
        UpdateStatusEffects(deltaTime);

        // Update animation
        UpdateAnimation(deltaTime);

        // Reset state if not in combat for a while
        if (State == WorldObjectState.Attacking || State == WorldObjectState.Casting)
        {
            var timeSinceAction = Math.Max(
                (DateTime.Now - LastAttackTime).TotalSeconds,
                (DateTime.Now - LastSpellTime).TotalSeconds
            );

            if (timeSinceAction > 2.0) // 2 seconds to return to idle
            {
                SetState(WorldObjectState.Idle);
            }
        }
    }

    private void UpdateStatusEffects(float deltaTime)
    {
        var expiredEffects = new List<StatusEffect>();

        foreach (var effect in StatusEffects)
        {
            effect.Update(deltaTime);
                
            // Handle poison ticks
            if (effect is PoisonEffect poisonEffect)
            {
                poisonEffect.ProcessTick(this);
            }

            if (effect.IsExpired)
            {
                expiredEffects.Add(effect);
            }
        }

        // Remove expired effects
        foreach (var effect in expiredEffects)
        {
            RemoveStatusEffect(effect);
        }
    }

    private void UpdateAnimation(float deltaTime)
    {
        var now = DateTime.Now;
        if ((now - LastAnimationUpdate).TotalMilliseconds > 100) // 10 FPS animation
        {
            AnimationFrame = (short)((AnimationFrame + 1) % 4); // 4-frame animation
            LastAnimationUpdate = now;
        }
    }

    public virtual bool TakeDamage(int damage, WorldObject_Living? attacker = null, DamageType damageType = DamageType.Physical)
    {
        if (State == WorldObjectState.Dead || Health <= 0) return false;

        // Calculate actual damage after armor/resistance
        var actualDamage = CalculateDamageAfterDefenses(damage, damageType);
            
        if (actualDamage <= 0) return false;

        var oldHealth = Health;
        Health = Math.Max(0, Health - actualDamage);

        DamageTaken?.Invoke(this, new DamageEventArgs(this, attacker, actualDamage, damageType));

        // Check for death
        if (Health <= 0 && oldHealth > 0)
        {
            OnDeath(attacker);
        }

        return true;
    }

    public virtual void Heal(int amount, WorldObject_Living? healer = null)
    {
        if (State == WorldObjectState.Dead || Health <= 0) return;

        var oldHealth = Health;
        Health = Math.Min(MaxHealth, Health + amount);
        var actualHealing = Health - oldHealth;

        if (actualHealing > 0)
        {
            HealthRestored?.Invoke(this, new HealEventArgs(this, healer, actualHealing));
        }
    }

    public virtual void RestoreMana(int amount)
    {
        Mana = Math.Min(MaxMana, Mana + amount);
    }

    private int CalculateDamageAfterDefenses(int baseDamage, DamageType damageType)
    {
        var damage = baseDamage;

        switch (damageType)
        {
        case DamageType.Physical:
            // Armor class reduces physical damage
            var damageReduction = ArmorClass * 0.5f;
            damage = (int)Math.Max(1, damage - damageReduction);
            break;

        case DamageType.Magical:
            // Magic resistance reduces magical damage
            var magicReduction = MagicResistance * 0.01f; // Percentage reduction
            damage = (int)Math.Max(1, damage * (1.0f - magicReduction));
            break;

        case DamageType.True:
            // True damage ignores all defenses
            break;
        }

        return damage;
    }

    public virtual int CalculateAttackDamage()
    {
        var baseDamage = Damage + (Strength / 2);
            
        // Add some randomness (80-120% of base damage)
        var random = new Random();
        var multiplier = 0.8f + (random.NextSingle() * 0.4f);
            
        return Math.Max(1, (int)(baseDamage * multiplier));
    }

    public virtual float CalculateHitChance(WorldObject_Living target)
    {
        var attackerHit = Hit + (Dexterity / 2);
        var targetDodge = target.Dexterity + (target.ArmorClass / 2);
            
        // Base 70% hit chance, modified by stats
        var hitChance = 0.7f + ((attackerHit - targetDodge) * 0.01f);
            
        return Math.Clamp(hitChance, 0.05f, 0.95f); // 5-95% hit chance
    }

    public virtual void AddStatusEffect(StatusEffect effect)
    {
        if (effect == null) return;

        // Check if we already have this type of effect
        var existingEffect = StatusEffects.FirstOrDefault(e => e.Type == effect.Type);
        if (existingEffect != null)
        {
            // Try to stack
            existingEffect.Stack(effect);
            return;
        }

        // Add new effect
        StatusEffects.Add(effect);
        effect.Apply(this);
        StatusEffectAdded?.Invoke(this, new StatusEffectEventArgs(effect));
    }

    public virtual void RemoveStatusEffect(StatusEffect effect)
    {
        if (StatusEffects.Remove(effect))
        {
            effect.Remove(this);
            StatusEffectRemoved?.Invoke(this, new StatusEffectEventArgs(effect));
        }
    }

    public virtual void RemoveStatusEffectByType(StatusEffectType type)
    {
        var effectsToRemove = StatusEffects.Where(e => e.Type == type).ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveStatusEffect(effect);
        }
    }

    public virtual bool HasStatusEffect(StatusEffectType type)
    {
        return StatusEffects.Any(e => e.Type == type);
    }

    public virtual bool CanAttack()
    {
        if (State == WorldObjectState.Dead || HasStatusEffect(StatusEffectType.Stun))
            return false;

        var timeSinceLastAttack = DateTime.Now - LastAttackTime;
        return timeSinceLastAttack.TotalSeconds >= (1.0 / AttackSpeed);
    }

    public virtual bool CanCast()
    {
        if (State == WorldObjectState.Dead || HasStatusEffect(StatusEffectType.Silence))
            return false;

        var timeSinceLastSpell = DateTime.Now - LastSpellTime;
        return timeSinceLastSpell.TotalSeconds >= (1.0 / CastSpeed);
    }

    public virtual bool CanMove()
    {
        return State != WorldObjectState.Dead && 
                !HasStatusEffect(StatusEffectType.Stun) &&
                !HasStatusEffect(StatusEffectType.Freeze) &&
                !HasStatusEffect(StatusEffectType.Sleep);
    }

    public virtual bool Attack(WorldObject_Living target)
    {
        if (!CanAttack() || target == null || target.IsDisposed) return false;

        LastAttackTime = DateTime.Now;
        SetState(WorldObjectState.Attacking);

        // Calculate hit chance
        var hitChance = CalculateHitChance(target);
        var random = new Random();
            
        if (random.NextDouble() <= hitChance)
        {
            var damage = CalculateAttackDamage();
            return target.TakeDamage(damage, this);
        }

        return false; // Miss
    }

    public virtual void SetTarget(WorldObject_Living? newTarget)
    {
        if (Target != newTarget)
        {
            Target = newTarget;
            TargetChanged?.Invoke(this, newTarget);
        }
    }

    public virtual void ClearTarget()
    {
        SetTarget(null);
    }

    public virtual bool IsHostileTo(WorldObject_Living other)
    {
        // Override in derived classes for faction/alignment logic
        return false;
    }

    public virtual bool IsFriendlyTo(WorldObject_Living other)
    {
        // Override in derived classes for faction/alignment logic
        return !IsHostileTo(other);
    }

    protected virtual void OnDeath(WorldObject_Living? killer)
    {
        SetState(WorldObjectState.Dead);
        Health = 0;
        ClearTarget();
            
        // Clear most status effects on death
        var effectsToRemove = StatusEffects.Where(e => e.Type != StatusEffectType.Curse).ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveStatusEffect(effect);
        }

        Died?.Invoke(this, killer);
    }

    public virtual void Resurrect(int healthPercent = 25)
    {
        if (State != WorldObjectState.Dead) return;

        Health = Math.Max(1, (MaxHealth * healthPercent) / 100);
        Mana = Math.Max(1, (MaxMana * healthPercent) / 100);
        SetState(WorldObjectState.Idle);
    }

    public virtual void ModifyAttribute(string attributeName, int modifier)
    {
        switch (attributeName.ToLower())
        {
        case "strength":
            Strength = Math.Max(1, Strength + modifier);
            break;
        case "intelligence":
            Intelligence = Math.Max(1, Intelligence + modifier);
            break;
        case "wisdom":
            Wisdom = Math.Max(1, Wisdom + modifier);
            break;
        case "constitution":
            Constitution = Math.Max(1, Constitution + modifier);
            // Update max health based on constitution
            var healthChange = modifier * 2;
            MaxHealth = Math.Max(10, MaxHealth + healthChange);
            if (Health > MaxHealth) Health = MaxHealth;
            break;
        case "dexterity":
            Dexterity = Math.Max(1, Dexterity + modifier);
            break;
        case "ac":
        case "armorclass":
            ArmorClass = Math.Max(0, ArmorClass + modifier);
            break;
        case "damage":
            Damage = Math.Max(0, Damage + modifier);
            break;
        case "hit":
            Hit = Math.Max(0, Hit + modifier);
            break;
        case "magicresistance":
            MagicResistance = Math.Clamp(MagicResistance + modifier, 0, 100);
            break;
        }
    }

    public virtual Dictionary<string, object> GetStatus()
    {
        return new Dictionary<string, object>
        {
            ["Health"] = $"{Health}/{MaxHealth}",
            ["Mana"] = $"{Mana}/{MaxMana}",
            ["Level"] = Level,
            ["Experience"] = Experience,
            ["Strength"] = Strength,
            ["Intelligence"] = Intelligence,
            ["Wisdom"] = Wisdom,
            ["Constitution"] = Constitution,
            ["Dexterity"] = Dexterity,
            ["ArmorClass"] = ArmorClass,
            ["MagicResistance"] = MagicResistance,
            ["State"] = State.ToString(),
            ["StatusEffects"] = StatusEffects.Count,
            ["Target"] = Target?.Name ?? "None"
        };
    }

    public override void Dispose()
    {
        StatusEffects.Clear();
        ClearTarget();
        base.Dispose();
    }
}