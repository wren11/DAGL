using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DarkAges.Library.Graphics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic;

public class Item : IEquatable<Item>, ICloneable
{
    public event EventHandler<ItemEventArgs> ItemUsed;
    public event EventHandler<ItemEventArgs> ItemEquipped;
    public event EventHandler<ItemEventArgs> ItemUnequipped;
    public event EventHandler<ItemEventArgs> DurabilityChanged;
    public event EventHandler<ItemEventArgs> QuantityChanged;

    public int Id { get; set; }
    public int ItemId
    {
        get => Id;
        init => Id = value;
    }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ItemType Type { get; set; }
    public ItemCategory Category { get; set; }
    public ItemRarity Rarity { get; set; }
    public int Level { get; set; }
    public int Value { get; set; }
    public int BasePrice { get; set; }
    public int Weight { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
    public bool IsStackable { get; set; }
    public bool IsEquippable { get; set; }
    public bool IsConsumable { get; set; }
    public bool IsQuestItem { get; set; }
    public bool IsTradeable { get; set; }
    public bool IsSellable { get; set; }
    public bool IsDroppable { get; set; }
    public bool IsDestroyable { get; set; }
    public bool IsMagical { get; set; }
    public int Durability { get; private set; }
    public int MaxDurability { get; private set; }
    public IndexedImage? Icon { get; set; }
    public List<ItemEffect> Effects { get; set; } = [];
    public Dictionary<string, int> Stats { get; set; } = [];
    public Dictionary<string, int> Requirements { get; set; } = [];
    public Dictionary<string, int> StatBonuses { get; set; } = [];
    public EquipSlot EquipSlot { get; set; }
    public int RequiredLevel { get; set; }
    public string RequiredClass { get; set; } = "";
    public int RequiredStrength { get; set; }
    public int RequiredIntelligence { get; set; }
    public int RequiredWisdom { get; set; }
    public int RequiredConstitution { get; set; }
    public int RequiredDexterity { get; set; }
    public bool HasDurability { get; set; }
    public float DurabilityPercent => MaxDurability > 0 ? (float)Durability / MaxDurability : 0;

    public Item()
    {
        Durability = 100;
        MaxDurability = 100;
    }

    public Item(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
        Durability = 100;
        MaxDurability = 100;
    }

    public bool CanStackWith(Item item)
    {
        return IsStackable && item.IsStackable && ItemId == item.ItemId;
    }

    public void Use(WorldObject_Human user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (!IsConsumable) return;

        // Apply effects
        foreach (var effect in Effects)
        {
            effect.Apply(user);
        }

        // Reduce quantity
        if (IsStackable)
        {
            Quantity--;
            OnQuantityChanged();
        }

        OnItemUsed();
    }

    public void Equip(WorldObject_Human user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (!IsEquippable) return;

        // Apply stats
        foreach (var stat in Stats)
        {
            user.ModifyStat(stat.Key, stat.Value);
        }

        OnItemEquipped();
    }

    public void Unequip(WorldObject_Human user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (!IsEquippable) return;

        // Remove stats
        foreach (var stat in Stats)
        {
            user.ModifyStat(stat.Key, -stat.Value);
        }

        OnItemUnequipped();
    }

    public void ModifyDurability(int amount)
    {
        var newDurability = Math.Clamp(Durability + amount, 0, MaxDurability);
        if (newDurability != Durability)
        {
            Durability = newDurability;
            OnDurabilityChanged();
        }
    }

    public void SetMaxDurability(int maxDurability)
    {
        if (maxDurability <= 0) throw new ArgumentException("Max durability must be greater than 0", nameof(maxDurability));

        MaxDurability = maxDurability;
        Durability = Math.Min(Durability, MaxDurability);
        OnDurabilityChanged();
    }

    public void Repair()
    {
        if (Durability < MaxDurability)
        {
            Durability = MaxDurability;
            OnDurabilityChanged();
        }
    }

    public int GetDamage()
    {
        if (!Stats.TryGetValue("Damage", out var damage))
            return 0;

        // Apply durability scaling
        var durabilityFactor = Durability / (float)MaxDurability;
        return (int)(damage * durabilityFactor);
    }

    public bool MeetsRequirements(WorldObject_Human user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        foreach (var requirement in Requirements)
        {
            if (!user.CheckRequirement(requirement.Key, requirement.Value))
                return false;
        }

        return true;
    }

    protected virtual void OnItemUsed()
    {
        ItemUsed?.Invoke(this, new ItemEventArgs(this));
    }

    protected virtual void OnItemEquipped()
    {
        ItemEquipped?.Invoke(this, new ItemEventArgs(this));
    }

    protected virtual void OnItemUnequipped()
    {
        ItemUnequipped?.Invoke(this, new ItemEventArgs(this));
    }

    protected virtual void OnDurabilityChanged()
    {
        DurabilityChanged?.Invoke(this, new ItemEventArgs(this));
    }

    protected virtual void OnQuantityChanged()
    {
        QuantityChanged?.Invoke(this, new ItemEventArgs(this));
    }

    public override bool Equals(object obj)
    {
        if (obj is Item other)
            return Equals(other);
        return false;
    }

    public bool Equals(Item other)
    {
        if (other == null)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public object Clone()
    {
        var clone = (Item)MemberwiseClone();
        clone.Effects = Effects.Select(e => (ItemEffect)e.Clone()).ToList();
        clone.Stats = new Dictionary<string, int>(Stats);
        clone.Requirements = new Dictionary<string, int>(Requirements);
        return clone;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static Item FromJson(string json)
    {
        return JsonSerializer.Deserialize<Item>(json);
    }
}