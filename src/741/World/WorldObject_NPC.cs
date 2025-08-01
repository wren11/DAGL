using System;
using System.Collections.Generic;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.World;

/// <summary>
/// Represents a non-player character in the game world
/// </summary>
public class WorldObject_NPC : WorldObject_Living
{
    // NPC Identity
    public string Title { get; set; } = "";
    public NPCType NPCType { get; set; } = NPCType.Villager;
    public string Class { get; set; } = "";
        
    // Appearance (similar to humans)
    public short Gender { get; set; } = 0;
    public short HairStyle { get; set; } = 1;
    public short HairColor { get; set; } = 1;
    public short SkinColor { get; set; } = 1;
    public string Portrait { get; set; } = "";

    // Behavior
    public NPCAIType AIType { get; set; } = NPCAIType.Stationary;
    public Vector2 SpawnPosition { get; set; }
    public float MovementRange { get; set; } = 5.0f;
    public float AggroRange { get; set; } = 3.0f;
    public bool IsHostile { get; set; } = false;
    public bool IsImmortal { get; set; } = true;

    // Dialog system
    public readonly Dictionary<string, string> Dialogs = new();
    public string DefaultDialog { get; set; } = "Hello there!";
    public readonly List<NPCQuest> AvailableQuests = [];
    public readonly List<NPCPursuit> Pursuits = [];

    // Merchant functionality
    public bool IsMerchant { get; set; } = false;
    public readonly List<Item> MerchantInventory = [];
    public readonly List<string> BuyableItems = []; // Item types this NPC will buy
    public float BuyPriceMultiplier { get; set; } = 0.5f; // NPCs buy at 50% value
    public float SellPriceMultiplier { get; set; } = 1.5f; // NPCs sell at 150% value
    public int MerchantGold { get; set; } = 10000;

    // Services
    public readonly List<NPCService> Services = [];
    public bool CanRepair { get; set; } = false;
    public bool CanTeach { get; set; } = false;
    public readonly List<string> TeachableSkills = [];
    public readonly List<string> TeachableSpells = [];

    // Respawn
    public bool CanRespawn { get; set; } = true;
    public float RespawnTime { get; set; } = 300.0f; // 5 minutes
    public DateTime DeathTime { get; set; }

    // AI state
    private NPCAIState _aiState = NPCAIState.Idle;

    // Events
    public event EventHandler<FindNearestPlayerEventArgs>? OnFindNearestPlayerRequested;
    private DateTime _lastAIUpdate = DateTime.Now;
    private Vector2 _aiTarget;
    private WorldObject_Living? _currentTarget;
    private DateTime _lastDialogTime = DateTime.Now;
    private readonly Queue<Vector2> _patrolPoints = new();

    public WorldObject_NPC() : base(WorldObjectType.NPC)
    {
        SpawnPosition = Position;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!IsActive || IsDisposed) return;

        // Update AI
        UpdateAI(deltaTime);

        // Handle respawn
        if (State == WorldObjectState.Dead && CanRespawn)
        {
            HandleRespawn();
        }

        // Merchant inventory refresh
        if (IsMerchant && DateTime.Now.Hour != _lastAIUpdate.Hour)
        {
            RefreshMerchantInventory();
        }

        _lastAIUpdate = DateTime.Now;
    }

    protected void UpdateAI(float deltaTime)
    {
        if (State == WorldObjectState.Dead) return;

        switch (AIType)
        {
        case NPCAIType.Stationary:
            UpdateStationaryAI();
            break;
        case NPCAIType.Wandering:
            UpdateWanderingAI(deltaTime);
            break;
        case NPCAIType.Patrolling:
            UpdatePatrollingAI(deltaTime);
            break;
        case NPCAIType.Aggressive:
            UpdateAggressiveAI(deltaTime);
            break;
        case NPCAIType.Guard:
            UpdateGuardAI(deltaTime);
            break;
        }
    }

    private void UpdateStationaryAI()
    {
        // Stationary NPCs do not move, but can interact
        if (_aiState != NPCAIState.Idle)
        {
            _aiState = NPCAIState.Idle;
            SetState(WorldObjectState.Idle);
        }
        // Check for player interaction
        if (DateTime.Now - _lastDialogTime > TimeSpan.FromSeconds(5)) // Allow interaction every 5 seconds
        {
            _lastDialogTime = DateTime.Now;
            // Trigger dialog or interaction logic here
            Console.WriteLine($"{Name} is ready to interact.");
        }
    }

    private void UpdateWanderingAI(float deltaTime)
    {
        switch (_aiState)
        {
        case NPCAIState.Idle:
            if (new Random().NextDouble() < 0.02f) // 2% chance per frame to start moving
            {
                PickRandomTarget();
                _aiState = NPCAIState.Moving;
            }
            break;

        case NPCAIState.Moving:
            MoveTowardsTarget(deltaTime);
            if (Vector2.Distance(Position, _aiTarget) < 1.0f)
            {
                _aiState = NPCAIState.Idle;
                SetState(WorldObjectState.Idle);
            }
            break;
        }
    }

    private void UpdatePatrollingAI(float deltaTime)
    {
        if (_patrolPoints.Count == 0) return;

        switch (_aiState)
        {
        case NPCAIState.Idle:
            if (_patrolPoints.Count > 0)
            {
                _aiTarget = _patrolPoints.Dequeue();
                _aiState = NPCAIState.Moving;
                SetState(WorldObjectState.Moving);
            }
            break;

        case NPCAIState.Moving:
            MoveTowardsTarget(deltaTime);
            if (Vector2.Distance(Position, _aiTarget) < 1.0f)
            {
                _patrolPoints.Enqueue(_aiTarget); // Add back to end of patrol
                _aiState = NPCAIState.Idle;
                SetState(WorldObjectState.Idle);
            }
            break;
        }
    }

    private void UpdateAggressiveAI(float deltaTime)
    {
        if (!IsHostile) return;

        // Look for nearby players to attack
        if (_currentTarget == null || _currentTarget.IsDisposed || _currentTarget.State == WorldObjectState.Dead)
        {
            FindNearestEnemy();
        }

        if (_currentTarget != null)
        {
            var distance = GetDistanceTo(_currentTarget);
                
            if (distance > AggroRange * 2) // Lost target
            {
                _currentTarget = null;
                _aiState = NPCAIState.Idle;
                MoveTo(new System.Numerics.Vector2(SpawnPosition.X, SpawnPosition.Y));
            }
            else if (distance > 1.5f) // Move closer
            {
                _aiTarget = _currentTarget.Position;
                MoveTowardsTarget(deltaTime);
                SetState(WorldObjectState.Moving);
            }
            else if (CanAttack()) // Attack
            {
                Attack(_currentTarget);
            }
        }
    }

    private void UpdateGuardAI(float deltaTime)
    {
        // Similar to aggressive but with different targeting rules
        UpdateAggressiveAI(deltaTime);
    }

    private void PickRandomTarget()
    {
        var random = new Random();
        var angle = random.NextDouble() * Math.PI * 2;
        var distance = random.NextDouble() * MovementRange;
            
        _aiTarget = SpawnPosition + new Vector2(
            (float)(Math.Cos(angle) * distance),
            (float)(Math.Sin(angle) * distance)
        );
    }

    private void MoveTowardsTarget(float deltaTime)
    {
        if (_aiTarget == Vector2.Zero) return;
        var direction = _aiTarget - Position;
        if (direction.Length() < 0.1f) return; // Already at target
        direction.Normalize();
        var speed = GetMovementSpeed();
        Position += direction * speed * deltaTime;
        // Update facing direction
        FacingDirection = CalculateDirection(direction);
        SetState(WorldObjectState.Moving);
    }

    private int GetMovementSpeed()
    {
        // Calculate movement speed based on NPC type and state
        switch (NPCType)
        {
            case NPCType.Villager:
                return 1; // Slow
            case NPCType.Merchant:
                return 2; // Medium
            case NPCType.Guard:
                return 3; // Fast
            default:
                return 1; // Default slow speed
        }
    }

    public byte FacingDirection { get; set; }


    private byte CalculateDirection(Vector2 direction)
    {
        // Convert direction vector to 8-directional facing
        var angle = Math.Atan2(direction.Y, direction.X);
        var degrees = angle * 180.0 / Math.PI;
        if (degrees < 0) degrees += 360;
            
        return (byte)(((int)(degrees + 22.5) / 45) % 8 + 1);
    }

    private void FindNearestEnemy()
    {
        if (!IsHostile) return;

        WorldObject_Human? nearestPlayer = null;
        var nearestDistance = float.MaxValue;

        // This would typically be handled by the world manager
        // For now, we'll use an event-based approach
        var args = new FindNearestPlayerEventArgs
        {
            NpcPosition = Position,
            AggroRange = AggroRange,
            RequestingNpc = this
        };

        OnFindNearestPlayerRequested?.Invoke(this, args);
            
        if (args.NearestPlayer != null)
        {
            _currentTarget = args.NearestPlayer;
            _aiState = NPCAIState.Chasing;
            SetState(WorldObjectState.Moving);
        }
    }

    public string GetDialog(WorldObject_Human player, string dialogKey = "")
    {
        if (string.IsNullOrEmpty(dialogKey))
            dialogKey = "default";

        if (Dialogs.TryGetValue(dialogKey, out var dialog))
        {
            return ProcessDialogVariables(dialog, player);
        }

        return ProcessDialogVariables(DefaultDialog, player);
    }

    private string ProcessDialogVariables(string dialog, WorldObject_Human player)
    {
        // Replace variables in dialog text
        return dialog
                .Replace("{player}", player.Name)
                .Replace("{npc}", Name)
                .Replace("{time}", DateTime.Now.ToString("HH:mm"));
    }

    public bool CanSellItem(Item item)
    {
        return IsMerchant && MerchantInventory.Contains(item);
    }

    public bool CanBuyItem(Item item)
    {
        return IsMerchant && BuyableItems.Contains(item.GetType().Name) && MerchantGold > 0;
    }

    public int GetSellPrice(Item item)
    {
        return (int)(item.Value * SellPriceMultiplier);
    }

    public int GetBuyPrice(Item item)
    {
        return (int)(item.Value * BuyPriceMultiplier);
    }

    public bool SellItemToPlayer(Item item, WorldObject_Human player)
    {
        if (!CanSellItem(item)) return false;
            
        var price = GetSellPrice(item);
        if (!player.SpendGold(price)) return false;

        MerchantInventory.Remove(item);
        player.AddItem(item);
        MerchantGold += price;

        return true;
    }

    public bool BuyItemFromPlayer(Item item, WorldObject_Human player)
    {
        if (!CanBuyItem(item)) return false;
            
        var price = GetBuyPrice(item);
        if (MerchantGold < price) return false;

        player.RemoveItem(item);
        player.AddGold(price);
        MerchantGold -= price;

        // Optionally add to merchant inventory
        if (MerchantInventory.Count < 100) // Arbitrary limit
        {
            MerchantInventory.Add(item);
        }

        return true;
    }

    public void AddPatrolPoint(Vector2 point)
    {
        _patrolPoints.Enqueue(point);
    }

    public void SetDialog(string key, string text)
    {
        Dialogs[key] = text;
    }

    public void AddService(NPCService service)
    {
        Services.Add(service);
    }

    //TODO: this method should be called periodically to refresh the merchant's inventory
    private void RefreshMerchantInventory()
    {
        if (!IsMerchant) return;

        MerchantInventory.Clear();

        // Load merchant inventory based on shop type
        if (string.IsNullOrEmpty(Class))
        {
            Class = "general"; // Default to general shop if no class specified
        }
        switch (Class.ToLowerInvariant())
        {
        case "weapons":
            LoadWeaponShopInventory();
            break;
        case "armor":
            LoadArmorShopInventory();
            break;
        case "general":
            LoadGeneralShopInventory();
            break;
        case "magic":
            LoadMagicShopInventory();
            break;
        case "alchemy":
            LoadAlchemyShopInventory();
            break;
        default:
            LoadDefaultShopInventory();
            break;
        }

        LastInventoryRefresh = DateTime.Now;
    }

    public DateTime LastInventoryRefresh { get; set; }

    private void HandleRespawn()
    {
        if ((DateTime.Now - DeathTime).TotalSeconds >= RespawnTime)
        {
            // Respawn the NPC
            Health = MaxHealth;
            Mana = MaxMana;
            Position = SpawnPosition;
            SetState(WorldObjectState.Idle);
            _currentTarget = null;
            _aiState = NPCAIState.Idle;
                
            Console.WriteLine($"NPC {Name} respawned");
        }
    }

    protected override void OnDeath(WorldObject_Living? killer)
    {
        base.OnDeath(killer);
            
        DeathTime = DateTime.Now;
        _currentTarget = null;
        _aiState = NPCAIState.Dead;
            
        // Give experience to killer
        if (killer is WorldObject_Human player)
        {
            var expReward = Level * 10;
            player.AddExperience(expReward);
        }
    }

    public override void Dispose()
    {
        MerchantInventory.Clear();
        AvailableQuests.Clear();
        Services.Clear();
        TeachableSkills.Clear();
        TeachableSpells.Clear();
        Dialogs.Clear();
            
        base.Dispose();
    }

    private void LoadWeaponShopInventory()
    {
        // Add common weapons
        MerchantInventory.Add(new Item { Name = "Iron Sword", ItemId = 1001, BasePrice = 100, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Steel Dagger", ItemId = 1002, BasePrice = 80, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Oak Staff", ItemId = 1003, BasePrice = 120, Rarity = ItemRarity.Common });
    }

    private void LoadArmorShopInventory()
    {
        // Add common armor
        MerchantInventory.Add(new Item { Name = "Leather Helmet", ItemId = 2001, BasePrice = 50, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Chain Mail", ItemId = 2002, BasePrice = 200, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Iron Shield", ItemId = 2003, BasePrice = 150, Rarity = ItemRarity.Common });
    }

    private void LoadGeneralShopInventory()
    {
        // Add general items
        MerchantInventory.Add(new Item { Name = "Health Potion", ItemId = 3001, BasePrice = 20, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Mana Potion", ItemId = 3002, BasePrice = 25, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Bread", ItemId = 3003, BasePrice = 5, Rarity = ItemRarity.Common });
    }

    private void LoadMagicShopInventory()
    {
        // Add magic items
        MerchantInventory.Add(new Item { Name = "Magic Crystal", ItemId = 4001, BasePrice = 300, Rarity = ItemRarity.Uncommon });
        MerchantInventory.Add(new Item { Name = "Spell Scroll", ItemId = 4002, BasePrice = 150, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Enchanted Ring", ItemId = 4003, BasePrice = 500, Rarity = ItemRarity.Rare });
    }

    private void LoadAlchemyShopInventory()
    {
        // Add alchemy items
        MerchantInventory.Add(new Item { Name = "Herb Bundle", ItemId = 5001, BasePrice = 30, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Empty Vial", ItemId = 5002, BasePrice = 10, Rarity = ItemRarity.Common });
        MerchantInventory.Add(new Item { Name = "Transmutation Kit", ItemId = 5003, BasePrice = 250, Rarity = ItemRarity.Uncommon });
    }

    private void LoadDefaultShopInventory()
    {
        // Add default items for unspecified shop types
        LoadGeneralShopInventory();
    }
}