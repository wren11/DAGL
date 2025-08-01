using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;

namespace DarkAges.Library.World;

/// <summary>
/// Represents a human player character in the game world
/// </summary>
public class WorldObject_Human : WorldObject_Living
{
    // Appearance
    public short Gender { get; set; } = 0; // 0 = male, 1 = female
    public short HairStyle { get; set; } = 1;
    public short HairColor { get; set; } = 1;
    public short SkinColor { get; set; } = 1;

    // Character class and paths
    public string ClassName { get; set; } = "Peasant";
    public string Path { get; set; } = "";
    public int PathLevel { get; set; } = 0;
    public string Class { get; set; } = "Peasant";
    public string CurrentMap { get; set; } = "Mileth";

    // Equipment
    public readonly Dictionary<EquipSlot, Item> Equipment = new();
    public readonly List<Item> Inventory = [];
    public int MaxInventorySlots { get; set; } = 60;
    public int Gold { get; set; } = 0;

    // Skills and Spells
    public readonly Dictionary<int, PlayerSkill> Skills = new();
    public readonly Dictionary<int, PlayerSpell> Spells = new();
    public readonly List<int> AvailableSkills = [];
    public readonly List<int> AvailableSpells = [];

    // Player stats
    public int AbilityPoints { get; set; } = 0;
    public int SkillPoints { get; set; } = 0;
    public int SpellPoints { get; set; } = 0;
    public DateTime LastLogin { get; set; } = DateTime.Now;
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Legend and achievements
    public readonly List<LegendMark> LegendMarks = [];
    public readonly Dictionary<string, int> Achievements = new();

    // Social
    public readonly List<string> Friends = [];
    public readonly List<string> Ignored = [];
    public string Guild { get; set; } = "";
    public string GuildRank { get; set; } = "";

    // Game state
    public string LastMap { get; set; } = "Mileth";
    public Vector2 LastPosition { get; set; } = Vector2.Zero;
    public bool IsNewCharacter { get; set; } = true;
    public bool IsDirty { get; set; } = true; // Needs visual update

    // Exchange/Trading
    public bool IsInExchange { get; set; } = false;
    public WorldObject_Human? ExchangePartner { get; set; }
    public readonly List<Item> ExchangeItems = [];
    public int ExchangeGold { get; set; } = 0;
    public bool HasConfirmedExchange { get; set; } = false;

    // Events
    public event EventHandler<Item>? ItemEquipped;
    public event EventHandler<Item>? ItemUnequipped;
    public event EventHandler<Item>? ItemAdded;
    public event EventHandler<Item>? ItemRemoved;
    public event EventHandler<LegendMark>? LegendMarkAdded;
    public event EventHandler<PlayerSkill>? SkillLearned;
    public event EventHandler<PlayerSpell>? SpellLearned;
    public event EventHandler<int>? ExperienceGained;
    public event EventHandler<int>? LeveledUp;
    public event EventHandler<WorldObject_Item>? OnItemDropped;

    public WorldObject_Human() : base(WorldObjectType.Human)
    {
        // Set default human stats
        MaxHealth = 100;
        Health = MaxHealth;
        MaxMana = 50;
        Mana = MaxMana;
            
        // Initialize equipment slots
        foreach (var slot in Enum.GetValues<EquipSlot>())
        {
            Equipment[slot] = null;
        }

        // Initialize starting skills based on class
        InitializeStartingAbilities();

        // Set default appearance
        UpdateAppearance();
    }

    private void InitializeStartingAbilities()
    {
        switch (ClassName.ToLower())
        {
        case "peasant":
            // Basic starting skills
            LearnSkill(1, 0); // Assail
            break;
        case "warrior":
            LearnSkill(1, 0); // Assail
            LearnSkill(2, 0); // Strike
            break;
        case "rogue":
            LearnSkill(1, 0); // Assail
            LearnSkill(3, 0); // Sneak Attack
            break;
        case "wizard":
            LearnSpell(1, 0); // Srad (Fire)
            LearnSpell(2, 0); // Ioc (Heal)
            break;
        case "priest":
            LearnSpell(2, 0); // Ioc (Heal)
            LearnSpell(3, 0); // Ao Puinsein (Cure Poison)
            break;
        case "monk":
            LearnSkill(1, 0); // Assail
            LearnSkill(4, 0); // Kick
            break;
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!IsActive || IsDisposed) return;

        // Update equipment effects
        UpdateEquipmentEffects();

        // Update visual appearance if needed
        if (IsDirty)
        {
            UpdateAppearance();
            IsDirty = false;
        }

        // Update skill/spell cooldowns
        UpdateAbilityCooldowns(deltaTime);
    }

    private void UpdateAbilityCooldowns(float deltaTime)
    {
        foreach (var skill in Skills.Values)
        {
            if (skill.RemainingCooldown > 0)
            {
                skill.RemainingCooldown = Math.Max(0, skill.RemainingCooldown - deltaTime);
            }
        }

        foreach (var spell in Spells.Values)
        {
            if (spell.RemainingCooldown > 0)
            {
                spell.RemainingCooldown = Math.Max(0, spell.RemainingCooldown - deltaTime);
            }
        }
    }

    private void UpdateEquipmentEffects()
    {
        // Apply equipment bonuses
        // This would normally be cached and only recalculated when equipment changes
        var equipmentStats = CalculateEquipmentStats();
            
        // Apply bonuses (this is simplified - real implementation would be more complex)
        // Note: This is just for display/calculation - actual stats shouldn't be modified directly
    }

    private EquipmentStats CalculateEquipmentStats()
    {
        var stats = new EquipmentStats();
            
        foreach (var item in Equipment.Values)
        {
            if (item != null)
            {
                // Apply item stats (simplified)
                stats.BonusStrength += item.StatBonuses.GetValueOrDefault("Strength", 0);
                stats.BonusIntelligence += item.StatBonuses.GetValueOrDefault("Intelligence", 0);
                stats.BonusWisdom += item.StatBonuses.GetValueOrDefault("Wisdom", 0);
                stats.BonusConstitution += item.StatBonuses.GetValueOrDefault("Constitution", 0);
                stats.BonusDexterity += item.StatBonuses.GetValueOrDefault("Dexterity", 0);
                stats.BonusArmorClass += item.StatBonuses.GetValueOrDefault("AC", 0);
                stats.BonusDamage += item.StatBonuses.GetValueOrDefault("Damage", 0);
                stats.BonusHit += item.StatBonuses.GetValueOrDefault("Hit", 0);
            }
        }
            
        return stats;
    }

    public bool EquipItem(Item item, EquipSlot slot)
    {
        if (item == null || !CanEquipItem(item, slot)) return false;

        // Unequip current item in slot
        if (Equipment[slot] != null)
        {
            UnequipItem(slot);
        }

        // Remove from inventory
        if (!Inventory.Remove(item)) return false;

        // Equip the item
        Equipment[slot] = item;
        ItemEquipped?.Invoke(this, item);
        IsDirty = true;

        // Apply item effects
        ApplyItemEffects(item, true);

        return true;
    }

    public bool UnequipItem(EquipSlot slot)
    {
        var item = Equipment[slot];
        if (item == null) return false;

        // Check if inventory has space
        if (Inventory.Count >= MaxInventorySlots) return false;

        // Remove item effects
        ApplyItemEffects(item, false);

        // Unequip and add to inventory
        Equipment[slot] = null;
        Inventory.Add(item);
        ItemUnequipped?.Invoke(this, item);
        IsDirty = true;

        return true;
    }

    private bool CanEquipItem(Item item, EquipSlot slot)
    {
        // Check if item can be equipped in this slot
        if (!item.IsEquippable || item.EquipSlot != slot) return false;

        // Check level requirements
        if (Level < item.RequiredLevel) return false;

        // Check class requirements
        if (item.RequiredClass != null && item.RequiredClass != ClassName) return false;

        // Check stat requirements
        if (Strength < item.RequiredStrength) return false;
        if (Intelligence < item.RequiredIntelligence) return false;
        if (Wisdom < item.RequiredWisdom) return false;
        if (Constitution < item.RequiredConstitution) return false;
        if (Dexterity < item.RequiredDexterity) return false;

        return true;
    }

    private void ApplyItemEffects(Item item, bool apply)
    {
        var multiplier = apply ? 1 : -1;
            
        // Apply stat bonuses
        foreach (var bonus in item.StatBonuses)
        {
            switch (bonus.Key.ToLower())
            {
            case "strength":
                Strength += bonus.Value * multiplier;
                break;
            case "intelligence":
                Intelligence += bonus.Value * multiplier;
                break;
            case "wisdom":
                Wisdom += bonus.Value * multiplier;
                break;
            case "constitution":
                Constitution += bonus.Value * multiplier;
                break;
            case "dexterity":
                Dexterity += bonus.Value * multiplier;
                break;
            case "ac":
                ArmorClass += bonus.Value * multiplier;
                break;
            case "damage":
                Damage += bonus.Value * multiplier;
                break;
            case "hit":
                Hit += bonus.Value * multiplier;
                break;
            }
        }
    }

    public bool AddItem(Item item)
    {
        if (item == null || Inventory.Count >= MaxInventorySlots) return false;

        // Try to stack with existing items
        if (item.IsStackable)
        {
            var existingItem = Inventory.Find(i => i.CanStackWith(item));
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
                ItemAdded?.Invoke(this, item);
                return true;
            }
        }

        Inventory.Add(item);
        ItemAdded?.Invoke(this, item);
        return true;
    }

    public bool RemoveItem(Item item)
    {
        if (Inventory.Remove(item))
        {
            ItemRemoved?.Invoke(this, item);
            return true;
        }
        return false;
    }

    public void AddLegendMark(LegendMark mark)
    {
        LegendMarks.Add(mark);
        LegendMarkAdded?.Invoke(this, mark);
    }

    public void AddGold(int amount)
    {
        Gold = Math.Max(0, Gold + amount);
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }

    public void ModifyHitPoints(int amount)
    {
        var oldHealth = Health;
        Health = Math.Clamp(Health + amount, 0, MaxHealth);
            
        if (Health <= 0 && oldHealth > 0)
        {
            OnDeath(null);
        }
    }

    public void ModifyManaPoints(int amount)
    {
        Mana = Math.Clamp(Mana + amount, 0, MaxMana);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        Experience += amount;
        ExperienceGained?.Invoke(this, amount);

        // Check for level up
        while (CanLevelUp())
        {
            LevelUp();
        }
    }

    private bool CanLevelUp()
    {
        var requiredExp = CalculateRequiredExperience(Level + 1);
        return Experience >= requiredExp;
    }

    private int CalculateRequiredExperience(int level)
    {
        // Dark Ages-style experience formula
        return (int)(100 * Math.Pow(level, 2.5));
    }

    private void LevelUp()
    {
        var oldLevel = Level;
        Level++;
            
        // Increase health and mana based on class and constitution/intelligence
        var healthGain = 10 + (Constitution / 3);
        var manaGain = 5 + (Intelligence / 2);
            
        MaxHealth += healthGain;
        MaxMana += manaGain;
        Health = MaxHealth; // Full heal on level up
        Mana = MaxMana;

        // Grant stat points for distribution
        AbilityPoints += 2 + (Level / 10); // 2 base + bonus every 10 levels
            
        // Grant skill/spell points based on class
        switch (ClassName.ToLower())
        {
        case "warrior":
        case "monk":
            SkillPoints += 1 + (Level / 5);
            break;
        case "wizard":
        case "priest":
            SpellPoints += 1 + (Level / 5);
            break;
        case "rogue":
            SkillPoints += 1;
            if (Level % 3 == 0) SpellPoints += 1; // Some spells
            break;
        default:
            SkillPoints += 1;
            if (Level % 5 == 0) SpellPoints += 1; // Very few spells
            break;
        }

        IsDirty = true;
        LeveledUp?.Invoke(this, Level);
            
        Console.WriteLine($"{Name} leveled up to level {Level}!");
    }

    public bool LearnSkill(int skillId, int level = 0)
    {
        if (Skills.ContainsKey(skillId)) return false;

        var skill = new PlayerSkill
        {
            Id = skillId,
            Name = GetSkillName(skillId),
            Level = level,
            Experience = 0,
            MaxLevel = GetSkillMaxLevel(skillId),
            Cooldown = GetSkillCooldown(skillId),
            RemainingCooldown = 0,
            Requirements = GetSkillRequirements(skillId)
        };

        Skills[skillId] = skill;
        SkillLearned?.Invoke(this, skill);
        return true;
    }

    public bool LearnSpell(int spellId, int level = 0)
    {
        if (Spells.ContainsKey(spellId)) return false;

        var spell = new PlayerSpell
        {
            Id = spellId,
            Name = GetSpellName(spellId),
            Level = level,
            Experience = 0,
            MaxLevel = GetSpellMaxLevel(spellId),
            ManaCost = GetSpellManaCost(spellId),
            Cooldown = GetSpellCooldown(spellId),
            RemainingCooldown = 0,
            Element = GetSpellElement(spellId),
            Requirements = GetSpellRequirements(spellId)
        };

        Spells[spellId] = spell;
        SpellLearned?.Invoke(this, spell);
        return true;
    }

    public bool UseSkill(int skillId, WorldObject_Living? target = null)
    {
        if (!Skills.TryGetValue(skillId, out var skill)) return false;
        if (skill.RemainingCooldown > 0) return false;
        if (!CanAttack()) return false;

        var success = ExecuteSkill(skill, target);
        if (success)
        {
            skill.RemainingCooldown = skill.Cooldown;
            LastAttackTime = DateTime.Now;
            SetState(WorldObjectState.Attacking);
        }

        return success;
    }

    public bool CastSpell(int spellId, WorldObject_Living? target = null)
    {
        if (!Spells.TryGetValue(spellId, out var spell)) return false;
        if (spell.RemainingCooldown > 0) return false;
        if (Mana < spell.ManaCost) return false;
        if (!CanCast()) return false;

        var success = ExecuteSpell(spell, target);
        if (success)
        {
            Mana -= spell.ManaCost;
            spell.RemainingCooldown = spell.Cooldown;
            LastSpellTime = DateTime.Now;
            SetState(WorldObjectState.Casting);
        }

        return success;
    }

    public void ModifyStat(string key, int value)
    {
        switch (key.ToLower())
        {
            case "strength":
                Strength += value;
                break;
            case "intelligence":
                Intelligence += value;
                break;
            case "wisdom":
                Wisdom += value;
                break;
            case "constitution":
                Constitution += value;
                break;
            case "dexterity":
                Dexterity += value;
                break;
            case "ac":
                ArmorClass += value;
                break;
            case "damage":
                Damage += value;
                break;
            case "hit":
                Hit += value;
                break;
        }
    }

    public bool CheckRequirement(string key, int value)
    {
        return key.ToLower() switch
        {
            "strength" => Strength >= value,
            "intelligence" => Intelligence >= value,
            "wisdom" => Wisdom >= value,
            "constitution" => Constitution >= value,
            "dexterity" => Dexterity >= value,
            "level" => Level >= value,
            _ => true
        };
    }

    public void SetState(WorldObjectState state)
    {
        State = state;
    }

    private bool ExecuteSkill(PlayerSkill skill, WorldObject_Living? target)
    {
        switch (skill.Id)
        {
        case 1: // Assail
            return ExecuteAssail(target);
        case 2: // Strike
            return ExecuteStrike(target);
        case 3: // Sneak Attack
            return ExecuteSneakAttack(target);
        case 4: // Kick
            return ExecuteKick(target);
        default:
            return false;
        }
    }

    private bool ExecuteSpell(PlayerSpell spell, WorldObject_Living? target)
    {
        switch (spell.Id)
        {
        case 1: // Srad (Fire)
            return CastSrad(target);
        case 2: // Ioc (Heal)
            return CastIoc(target ?? this);
        case 3: // Ao Puinsein (Cure Poison)
            return CastAoPuinsein(target ?? this);
        default:
            return false;
        }
    }

    // Skill implementations
    private bool ExecuteAssail(WorldObject_Living? target)
    {
        if (target == null) return false;
        var damage = CalculateAttackDamage() + 5; // Base assail bonus
        return target.TakeDamage(damage, this);
    }

    private bool ExecuteStrike(WorldObject_Living? target)
    {
        if (target == null) return false;
        var damage = CalculateAttackDamage() + 15; // Higher damage
        return target.TakeDamage(damage, this);
    }

    private bool ExecuteSneakAttack(WorldObject_Living? target)
    {
        if (target == null) return false;
        var damage = CalculateAttackDamage() + 10;
        // Critical hit chance
        if (new Random().NextDouble() < 0.3)
        {
            damage *= 2;
        }
        return target.TakeDamage(damage, this);
    }

    private bool ExecuteKick(WorldObject_Living? target)
    {
        if (target == null) return false;
        var damage = CalculateAttackDamage() + 8;
        var success = target.TakeDamage(damage, this);
        // Chance to stun
        if (success && new Random().NextDouble() < 0.2)
        {
            var stunEffect = new StunEffect(2.0f);
            target.AddStatusEffect(stunEffect);
        }
        return success;
    }

    // Spell implementations
    private bool CastSrad(WorldObject_Living? target)
    {
        if (target == null) return false;
        var damage = 20 + (Intelligence / 2);
        return target.TakeDamage(damage, this, DamageType.Magical);
    }

    private bool CastIoc(WorldObject_Living target)
    {
        var healing = 25 + (Wisdom / 2);
        target.Heal(healing);
        return true;
    }

    private bool CastAoPuinsein(WorldObject_Living target)
    {
        target.RemoveStatusEffectByType(StatusEffectType.Poison);
        return true;
    }

    public void SetAppearance(short gender, short hairStyle, short hairColor, short skinColor)
    {
        Gender = gender;
        HairStyle = hairStyle;
        HairColor = hairColor;
        SkinColor = skinColor;
        IsDirty = true;
    }

    private void UpdateAppearance()
    {
        // Update the character's visual representation
        // This would typically involve composing the character image from base + hair + equipment
        var renderer = new HumanImageRenderer();
        renderer.SetCharacter(Gender, Direction, HairStyle, HairColor);
            
        // The sprite would be updated here
        // Sprite = renderer.ComposeCharacter(...);
    }

    public void TeleportTo(string mapName, Vector2 position)
    {
        LastMap = Name; // Current map becomes last map
        MapId = GetMapId(mapName); // Convert map name to ID
        Position = position;
        LastPosition = position;
    }

    private int GetMapId(string mapName)
    {
        // Simple mapping - in real implementation this would be from a map database
        return mapName.GetHashCode() & 0x7FFFFFFF; // Ensure positive
    }

    // Exchange/Trading methods
    public bool StartExchange(WorldObject_Human partner)
    {
        if (IsInExchange || partner.IsInExchange) return false;
            
        ExchangePartner = partner;
        partner.ExchangePartner = this;
        IsInExchange = true;
        partner.IsInExchange = true;
            
        return true;
    }

    public bool AddToExchange(Item item)
    {
        if (!IsInExchange || item == null) return false;
        if (!Inventory.Contains(item)) return false;
            
        ExchangeItems.Add(item);
        return true;
    }

    public bool AddGoldToExchange(int amount)
    {
        if (!IsInExchange || amount <= 0 || amount > Gold) return false;
            
        ExchangeGold += amount;
        return true;
    }

    public bool ConfirmExchange()
    {
        if (!IsInExchange || ExchangePartner == null) return false;
            
        HasConfirmedExchange = true;
            
        // If both have confirmed, complete the exchange
        if (ExchangePartner.HasConfirmedExchange)
        {
            return CompleteExchange();
        }
            
        return true;
    }

    private bool CompleteExchange()
    {
        if (ExchangePartner == null) return false;
            
        // Exchange items
        foreach (var item in ExchangeItems)
        {
            Inventory.Remove(item);
            ExchangePartner.Inventory.Add(item);
        }
            
        foreach (var item in ExchangePartner.ExchangeItems)
        {
            ExchangePartner.Inventory.Remove(item);
            Inventory.Add(item);
        }
            
        // Exchange gold
        Gold -= ExchangeGold;
        Gold += ExchangePartner.ExchangeGold;
        ExchangePartner.Gold -= ExchangePartner.ExchangeGold;
        ExchangePartner.Gold += ExchangeGold;
            
        CancelExchange();
        return true;
    }

    public void CancelExchange()
    {
        if (ExchangePartner != null)
        {
            ExchangePartner.IsInExchange = false;
            ExchangePartner.ExchangePartner = null;
            ExchangePartner.ExchangeItems.Clear();
            ExchangePartner.ExchangeGold = 0;
            ExchangePartner.HasConfirmedExchange = false;
        }
            
        IsInExchange = false;
        ExchangePartner = null;
        ExchangeItems.Clear();
        ExchangeGold = 0;
        HasConfirmedExchange = false;
    }

    public void SaveToDatabase()
    {
        LastLogin = DateTime.Now;
            
        var characterData = new
        {
            Name,
            Position = new { X = Position.X, Y = Position.Y },
            Level,
            Experience,
            Health,
            MaxHealth,
            Mana,
            MaxMana,
            Strength,
            Intelligence,
            Wisdom,
            Constitution,
            Dexterity,
            Gold,
            Class,
            LastLogin,
            CurrentMap,
            Inventory = Inventory.ConvertAll(item => new
            {
                item.Name,
                item.ItemId,
                item.Quantity,
                item.Durability,
                item.MaxDurability
            }),
            Equipment = Equipment.ToDictionary(kvp => kvp.Key.ToString(), kvp => new
            {
                kvp.Value.Name,
                kvp.Value.ItemId,
                kvp.Value.Durability,
                kvp.Value.MaxDurability
            }),
            Skills = Skills.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
            Spells = Spells.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value)
        };

        IO.DatabaseManager.SaveCharacterData(Name, characterData);
    }

    public static WorldObject_Human LoadFromDatabase(string characterName)
    {
        try
        {
            var characterData = IO.DatabaseManager.LoadCharacterData<dynamic>(characterName);
            if (characterData == null)
                return new WorldObject_Human { Name = characterName };

            var character = new WorldObject_Human
            {
                Name = characterData.name ?? characterName,
                Level = characterData.level ?? 1,
                Experience = characterData.experience ?? 0,
                Health = characterData.health ?? 100,
                MaxHealth = characterData.maxHealth ?? 100,
                Mana = characterData.mana ?? 100,
                MaxMana = characterData.maxMana ?? 100,
                Strength = characterData.strength ?? 10,
                Intelligence = characterData.intelligence ?? 10,
                Wisdom = characterData.wisdom ?? 10,
                Constitution = characterData.constitution ?? 10,
                Dexterity = characterData.dexterity ?? 10,
                Gold = characterData.gold ?? 0,
                Class = characterData.@class ?? "Peasant",
                CurrentMap = characterData.currentMap ?? "Mileth"
            };

            if (characterData.position != null)
            {
                character.Position = new Vector2(
                    characterData.position.x ?? 0,
                    characterData.position.y ?? 0
                );
            }

            if (characterData.lastLogin != null)
            {
                DateTime lastLogin;
                DateTime.TryParse(characterData.lastLogin.ToString(), out lastLogin);
                character.LastLogin = lastLogin;
            }

            return character;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading character {characterName}: {ex.Message}");
            return new WorldObject_Human { Name = characterName };
        }
    }

    protected override void OnDeath(WorldObject_Living? killer)
    {
        base.OnDeath(killer);

        // Handle player death
        // Drop items, teleport to temple, apply death penalty, etc.
        HandlePlayerDeath();
    }

    private void HandlePlayerDeath()
    {
        // Death penalty
        var expLoss = (int)(Experience * 0.1f); // Lose 10% experience
        Experience = Math.Max(0, Experience - expLoss);

        // Drop random items (simplified)
        var random = new Random();
        if (Inventory.Count > 0 && random.NextDouble() < 0.3) // 30% chance to drop item
        {
            var itemIndex = random.Next(Inventory.Count);
            var droppedItem = Inventory[itemIndex];
            RemoveItem(droppedItem);
                
            // Create WorldObject_Item for the dropped item
            var droppedWorldItem = new WorldObject_Item(droppedItem)
            {
                Position = Position,
                DropTime = DateTime.Now
            };
                
            // Add to world manager (this would be handled by the game's world system)
            OnItemDropped?.Invoke(this, droppedWorldItem);
        }

        // Teleport to temple (simplified)
        TeleportTo("Mileth", new Vector2(50, 50));
            
        // Restore some health
        Health = MaxHealth / 4;
        SetState(WorldObjectState.Idle);
    }

    // Helper methods for skill/spell data
    private string GetSkillName(int skillId)
    {
        return skillId switch
        {
            1 => "Assail",
            2 => "Strike",
            3 => "Sneak Attack",
            4 => "Kick",
            _ => "Unknown Skill"
        };
    }

    private string GetSpellName(int spellId)
    {
        return spellId switch
        {
            1 => "Srad",
            2 => "Ioc",
            3 => "Ao Puinsein",
            _ => "Unknown Spell"
        };
    }

    private int GetSkillMaxLevel(int skillId) => 100;
    private int GetSpellMaxLevel(int spellId) => 100;
    private float GetSkillCooldown(int skillId) => 1.0f;
    private float GetSpellCooldown(int spellId) => 2.0f;
    private int GetSpellManaCost(int spellId) => 5 + (spellId * 2);
    private SpellElement GetSpellElement(int spellId) => SpellElement.Fire;
    private Dictionary<string, int> GetSkillRequirements(int skillId) => new();
    private Dictionary<string, int> GetSpellRequirements(int spellId) => new();

    public override void Dispose()
    {
        // Save character data before disposal
        SaveToDatabase();
            
        // Cancel any ongoing exchange
        if (IsInExchange)
        {
            CancelExchange();
        }
            
        Equipment.Clear();
        Inventory.Clear();
        Skills.Clear();
        Spells.Clear();
        LegendMarks.Clear();
        Friends.Clear();
        Ignored.Clear();
            
        base.Dispose();
    }
}