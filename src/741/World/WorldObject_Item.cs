using System;
using System.Numerics;
using DarkAges.Library.GameLogic;
using System.Drawing;
using System.Globalization;

namespace DarkAges.Library.World;

/// <summary>
/// Represents an item object that exists in the game world
/// </summary>
public class WorldObject_Item : WorldObject
{
    // Dragging state
    public bool IsDragging { get; private set; }
    private Vector2 _dragStartPosition;

    // Item data
    public Item? Item { get; private set; }
    public string Owner { get; set; } = ""; // Who dropped this item
    public DateTime DropTime { get; set; } = DateTime.Now;
    public bool IsPublic { get; set; } = false; // Can anyone pick it up?
    public float LifeTime { get; set; } = 600.0f; // 10 minutes default
    public bool HasLifeTime { get; set; } = true;

    // Visual properties
    public bool IsGlowing { get; set; } = false;
    public float GlowIntensity { get; set; } = 1.0f;
    public string GlowColor { get; set; } = "White";

    // Pickup rules
    public int RequiredLevel { get; set; } = 0;
    public string RequiredClass { get; set; } = "";
    public bool CanBePickedUp { get; set; } = true;
    public bool IsQuestItem { get; set; } = false;

    // Events
    public event EventHandler<WorldObject_Human>? ItemPickedUp;
    public event EventHandler<WorldObject_Item>? ItemExpired;
    public event EventHandler? OnPlayerProximityCheck;

    public WorldObject_Item(Item item) : base(WorldObjectType.Item)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Name = item.Name ?? string.Empty;
            
        // Set visual properties based on item
        SetupVisuals();
            
        // Items are not collidable by default (players walk over them)
        IsCollidable = false;
            
        // Set bounding box for pickup detection
        BoundingBox = new System.Drawing.Rectangle(0, 0, 16, 16);
    }

    private void SetupVisuals()
    {
        if (Item == null) return;

        // Set sprite based on item icon
        if (Item.Icon != null)
        {
            // Convert icon to sprite
            // Sprite = ConvertIconToSprite(Item.Icon);
        }

        // Set glow for special items
        if (Item.Rarity >= ItemRarity.Rare)
        {
            IsGlowing = true;
            GlowColor = GetGlowColorForRarity(Item.Rarity);
            GlowIntensity = GetGlowIntensityForRarity(Item.Rarity);
        }

        // Magical items have special effects
        if (Item.IsMagical)
        {
            IsGlowing = true;
            GlowColor = "Blue";
        }
    }

    private string GetGlowColorForRarity(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Rare => "Blue",
            ItemRarity.Epic => "Purple",
            ItemRarity.Legendary => "Orange",
            ItemRarity.Artifact => "Red",
            _ => "White"
        };
    }

    private float GetGlowIntensityForRarity(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Rare => 0.5f,
            ItemRarity.Epic => 0.7f,
            ItemRarity.Legendary => 1.0f,
            ItemRarity.Artifact => 1.5f,
            _ => 0.3f
        };
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!IsActive || IsDisposed || Item is null) return;

        // Handle item expiration
        if (HasLifeTime)
        {
            var age = (float)(DateTime.Now - DropTime).TotalSeconds;
            if (age >= LifeTime)
            {
                ExpireItem();
                return;
            }

            // Visual effects as item ages
            if (age > LifeTime * 0.8f) // Last 20% of lifetime
            {
                // Make item blink to indicate it's about to expire
                var blinkRate = 2.0f; // Blinks per second
                var phase = (age * blinkRate) % 1.0f;
                Opacity = phase < 0.5f ? 1.0f : 0.5f;
            }
        }

        // Update glow effect
        if (IsGlowing)
        {
            UpdateGlowEffect(deltaTime);
        }

        // Check for nearby players
        CheckForPickup();
    }

    private void UpdateGlowEffect(float deltaTime)
    {
        // Animate glow intensity
        var time = (float)(DateTime.Now - DropTime).TotalSeconds;
        var pulse = (float)(Math.Sin(time * 2.0) * 0.2 + 0.8); // Pulse between 0.6 and 1.0
        // Apply pulse to visual system
    }

    private void Destroy()
    {
        Dispose();
    }

    private void CheckForPickup()
    {
        // Check for nearby players in the world
        // This would typically be handled by the world manager
        // For now, we'll emit an event that the world manager can handle
        OnPlayerProximityCheck?.Invoke(this, EventArgs.Empty);
    }

    public bool CanBePickedUpBy(WorldObject_Human player)
    {
        if (!CanBePickedUp || IsDisposed || Item is null) return false;

        // Check ownership
        if (!IsPublic && !string.IsNullOrEmpty(Owner) && Owner != player.Name)
        {
            // Item belongs to someone else and isn't public yet
            var age = (DateTime.Now - DropTime).TotalSeconds;
            if (age < 30.0f) // 30 second ownership protection
                return false;
        }

        // Check level requirement
        if (player.Level < RequiredLevel) return false;

        // Check class requirement
        if (!string.IsNullOrEmpty(RequiredClass) && RequiredClass != player.ClassName)
            return false;

        // Check inventory space
        if (player.Inventory.Count >= player.MaxInventorySlots && !Item.IsStackable)
            return false;

        // Check if item can stack with existing items
        if (Item.IsStackable)
        {
            var existingItem = player.Inventory.Find(i => i.CanStackWith(Item));
            if (existingItem == null && player.Inventory.Count >= player.MaxInventorySlots)
                return false;
        }

        return true;
    }

    public bool TryPickup(WorldObject_Human player)
    {
        if (!CanBePickedUpBy(player) || Item is null) return false;

        // Add item to player's inventory
        if (player.AddItem(Item))
        {
            ItemPickedUp?.Invoke(this, player);
                
            // Log pickup
            Console.WriteLine($"{player.Name} picked up {Item.Name}");
                
            // Remove from world
            Destroy();
            return true;
        }

        return false;
    }

    private void ExpireItem()
    {
        ItemExpired?.Invoke(this, this);
        if(Item is not null)
            Console.WriteLine($"Item {Item.Name} expired and disappeared from the world");
        Destroy();
    }

    public void SetOwnership(string ownerName, bool isPublic = false)
    {
        Owner = ownerName;
        IsPublic = isPublic;
        DropTime = DateTime.Now;
    }

    public void MakePublic()
    {
        IsPublic = true;
    }

    public void SetLifetime(float lifetime)
    {
        LifeTime = lifetime;
        HasLifeTime = lifetime > 0;
    }

    public void RemoveLifetime()
    {
        HasLifeTime = false;
    }

    public float GetRemainingLifetime()
    {
        if (!HasLifeTime) return float.MaxValue;
            
        var age = (float)(DateTime.Now - DropTime).TotalSeconds;
        return Math.Max(0, LifeTime - age);
    }

    public override void Render(Graphics.SpriteBatch spriteBatch)
    {
        if (!IsVisible || IsDisposed) return;

        base.Render(spriteBatch);

        // Render glow effect
        if (IsGlowing)
        {
            RenderGlowEffect(spriteBatch);
        }

        // Render item name above the item
        if (ShouldShowName())
        {
            RenderItemName(spriteBatch);
        }
    }

    private void RenderGlowEffect(Graphics.SpriteBatch spriteBatch)
    {
        if (!ShouldGlow()) return;

        var glowColor = GetGlowColor();
        var glowRadius = GetGlowRadius();
        var time = (float)(DateTime.Now - DropTime).TotalSeconds;
        var alpha = (float)(Math.Sin(time * 3.0) * 0.3 + 0.7); // Pulsing alpha

        // Draw multiple circles for glow effect
        for (var i = 0; i < 3; i++)
        {
            var radius = glowRadius + (i * 2);
            var currentAlpha = alpha * (1.0f - (i * 0.3f));
            var color = Color.FromArgb((int)(currentAlpha * 255), glowColor.R, glowColor.G, glowColor.B);
                
            spriteBatch.DrawCircle(Position, radius, color);
        }
    }

    private void RenderItemName(Graphics.SpriteBatch spriteBatch)
    {
        if (!ShouldShowName()) return;

        var displayName = GetDisplayName();
        var nameColor = GetNameColor();
        var font = Graphics.FontManager.Instance.GetFont("default");
            
        if (font != null)
        {
            var namePosition = new System.Numerics.Vector2(Position.X, Position.Y - 20);
            var time = (float)(DateTime.Now - DropTime).TotalSeconds;
            var bounce = (float)(Math.Sin(time * 2.0) * 2.0); // Slight bounce effect
                
            namePosition.Y += bounce;
                
            // Draw text shadow for better readability
            spriteBatch.DrawString(font, displayName, namePosition.X + 1, namePosition.Y + 1, 
                Color.Black);
                
            // Draw main text
            spriteBatch.DrawString(font, displayName, namePosition.X, namePosition.Y, 
                nameColor);
        }
    }

    private bool ShouldShowName()
    {
        // Show name for rare items, quest items, or when player is nearby
        return Item is not null && (Item.Rarity >= ItemRarity.Rare || IsQuestItem);
    }

    private bool ShouldGlow()
    {
        return Item is not null && (Item.Rarity >= ItemRarity.Uncommon || IsQuestItem);
    }

    private Color GetGlowColor()
    {
        return Item?.Rarity switch
        {
            ItemRarity.Common => Color.White,
            ItemRarity.Uncommon => Color.Green,
            ItemRarity.Rare => Color.Blue,
            ItemRarity.Epic => Color.Purple,
            ItemRarity.Legendary => Color.Orange,
            _ => Color.White
        };
    }

    private float GetGlowRadius()
    {
        return Item?.Rarity switch
        {
            ItemRarity.Uncommon => 10f,
            ItemRarity.Rare => 15f,
            ItemRarity.Epic => 20f,
            ItemRarity.Legendary => 25f,
            _ => 8f
        };
    }

    private string GetDisplayName()
    {
        if (Item is null) return string.Empty;
        
        if (IsQuestItem)
            return $"[Quest] {Item.Name}";
            
        return Item.Rarity switch
        {
            ItemRarity.Uncommon => $"◆ {Item.Name}",
            ItemRarity.Rare => $"★ {Item.Name}",
            ItemRarity.Epic => $"♦ {Item.Name}",
            ItemRarity.Legendary => $"♦♦ {Item.Name}",
            _ => Item.Name
        };
    }

    private Color GetNameColor()
    {
        if (IsQuestItem)
            return Color.Yellow;
                
        return Item.Rarity switch
        {
            ItemRarity.Common => Color.White,
            ItemRarity.Uncommon => Color.Green,
            ItemRarity.Rare => Color.Blue,
            ItemRarity.Epic => Color.Purple,
            ItemRarity.Legendary => Color.Orange,
            _ => Color.White
        };
    }

    public override void Dispose()
    {
        Item = null;
        base.Dispose();
    }

    public void GetItemInfo(out ushort itemId, out byte itemType, out int quantity, out int quality, out bool isStackable)
    {
        if (Item is null)
        {
            itemId = 0;
            itemType = 0;
            quantity = 0;
            quality = 0;
            isStackable = false;
            return;
        }
        
        itemId = (ushort)Item.Id;
        itemType = (byte)Item.Type;
        quantity = Item.Quantity;
        quality = Item.HasDurability ? (int)(Item.DurabilityPercent * 100) : 100;
        isStackable = Item.IsStackable;
    }

    public void StartDragging(int x, int y)
    {
        if (IsDragging) return;

        IsDragging = true;
        _dragStartPosition = new Vector2(x, y);
        // You might want to add some visual effect when dragging starts
    }

    public bool StopDragging(int x, int y)
    {
        if (!IsDragging) return false;

        IsDragging = false;
        // Here, you would typically check if the drop location is valid.
        // For example, is it on another player, a container, or just the ground?
        // Depending on the drop target, you might initiate a trade, a drop, or an inventory move.

        // For now, let's just assume dropping it on the ground.
        // We'll update the item's world position.
        Position = new Vector2(x, y);
            
        return true; // Indicate that the drop was "successful"
    }

    public void UpdateDragPosition(int x, int y)
    {
        if (!IsDragging) return;

        // This method would be called continuously as the mouse moves.
        // You could update the item's visual position to follow the cursor.
        Position = new Vector2(x, y);
    }

    // Static factory methods for common item drops
    public static WorldObject_Item CreateDrop(Item item, Vector2 position, string owner = "")
    {
        var worldItem = new WorldObject_Item(item)
        {
            Position = position,
            Owner = owner,
            IsPublic = string.IsNullOrEmpty(owner)
        };

        return worldItem;
    }

    public static WorldObject_Item CreateQuestItem(Item item, Vector2 position)
    {
        var worldItem = new WorldObject_Item(item)
        {
            Position = position,
            IsQuestItem = true,
            HasLifeTime = false, // Quest items don't expire
            IsGlowing = true,
            GlowColor = "Gold"
        };

        return worldItem;
    }

    public static WorldObject_Item CreateGoldDrop(int amount, Vector2 position, string owner = "")
    {
        var goldItem = new Item
        {
            Name = $"{amount} Gold",
            Value = amount,
            IsStackable = true,
            MaxQuantity = 1000000,
            Quantity = amount
        };

        var worldItem = new WorldObject_Item(goldItem)
        {
            Position = position,
            Owner = owner,
            IsPublic = string.IsNullOrEmpty(owner),
            IsGlowing = amount >= 1000, // Large gold piles glow
            GlowColor = "Yellow"
        };

        return worldItem;
    }
}