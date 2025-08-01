using System;
using System.Collections.Generic;
using DarkAges.Library.GameLogic;

namespace DarkAges.Library.GameLogic;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string HashedPassword { get; set; } = "";
    public string NCID { get; set; } = "";
    public string NCPassword { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime LastLoginDate { get; set; } = DateTime.Now;
    public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
    public bool IsOnline { get; set; } = false;
    public string LastIPAddress { get; set; } = "";
    public int LoginCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public string BanReason { get; set; } = "";
    public DateTime? BanExpiry { get; set; }
    public bool IsGameMaster { get; set; } = false;
    public List<Character> Characters { get; set; } = [];
    public List<Item> Inventory { get; set; } = [];
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Mana { get; set; } = 100;
    public int MaxMana { get; set; } = 100;
    public int Gold { get; set; } = 0;
    public short Gender { get; set; } = 0;
    public short HairStyle { get; set; } = 1;
    public short HairColor { get; set; } = 1;
    public short SkinColor { get; set; } = 1;

    // Character class and paths
    public string ClassName { get; set; } = "Peasant";
    public string Path { get; set; } = "";
    public int PathLevel { get; set; } = 0;

    // Stats
    public int Strength { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Dexterity { get; set; } = 10;

    // Visual update flag
    public bool IsDirty { get; set; } = true;

    // Social features
    public List<string> Friends { get; set; } = [];
    public List<string> Ignored { get; set; } = [];
    public string Guild { get; set; } = "";
    public string GuildRank { get; set; } = "";

    // Game progress
    public string LastMap { get; set; } = "Mileth";
    public int MapX { get; set; } = 50;
    public int MapY { get; set; } = 50;
    public bool IsNewCharacter { get; set; } = true;
    public object? TradeSession { get; set; }

    public User()
    {
    }

    public User(string username, string hashedPassword)
    {
        Username = username;
        HashedPassword = hashedPassword;
    }

    public virtual bool ValidatePassword(string password)
    {
        // In a real application, you would use a secure password hashing library like BCrypt.
        // For this example, we'll just compare the hashed passwords.
        return HashedPassword == password;
    }

    public virtual void AddItem(Item item)
    {
        Inventory.Add(item);
    }

    public virtual bool RemoveItem(Item item)
    {
        return Inventory.Remove(item);
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

    public void SetAppearance(short gender, short hairStyle, short hairColor, short skinColor)
    {
        Gender = gender;
        HairStyle = hairStyle;
        HairColor = hairColor;
        SkinColor = skinColor;
        IsDirty = true;
    }

    public int CalculateRequiredExperience(int level)
    {
        // Simple exponential formula for experience requirements
        return (int)(100 * Math.Pow(level, 2.2));
    }

    public bool CanLevelUp()
    {
        var requiredExp = CalculateRequiredExperience(Level + 1);
        return Experience >= requiredExp;
    }

    public void LevelUp()
    {
        if (!CanLevelUp()) return;

        Level++;
        MaxHealth += 10 + (Constitution / 2);
        MaxMana += 5 + (Intelligence / 3);
        Health = MaxHealth;
        Mana = MaxMana;

        // Grant stat points for distribution
        var statPoints = 2 + (Level / 10); // 2 base + 1 per 10 levels
        DistributeStatPoints(statPoints);

        IsDirty = true;
    }

    private void DistributeStatPoints(int points)
    {
        // Simple random distribution - in a real game, player would choose
        var random = new Random();
        for (var i = 0; i < points; i++)
        {
            switch (random.Next(5))
            {
            case 0: Strength++; break;
            case 1: Intelligence++; break;
            case 2: Wisdom++; break;
            case 3: Constitution++; break;
            case 4: Dexterity++; break;
            }
        }
    }

    public void SaveUserData()
    {
        LastLoginDate = DateTime.Now;
            
        var userData = new
        {
            Username,
            HashedPassword,
            CreationDate,
            LastLoginDate,
            TotalPlayTime,
            IsOnline,
            LastIPAddress,
            LoginCount,
            IsBanned,
            BanReason,
            BanExpiry,
            IsGameMaster,
            Characters = Characters.ConvertAll(c => c.Name)
        };

        IO.DatabaseManager.SaveUserData(Username, userData);
    }

    public static User LoadUserData(string username)
    {
        try
        {
            var userData = IO.DatabaseManager.LoadUserData<dynamic>(username);
            if (userData == null)
                return new User(username, "");

            var user = new User(userData.Username, userData.HashedPassword)
            {
                CreationDate = userData.CreationDate,
                LastLoginDate = userData.LastLoginDate,
                TotalPlayTime = userData.TotalPlayTime,
                IsOnline = userData.IsOnline,
                LastIPAddress = userData.LastIPAddress,
                LoginCount = userData.LoginCount,
                IsBanned = userData.IsBanned,
                BanReason = userData.BanReason,
                BanExpiry = userData.BanExpiry,
                IsGameMaster = userData.IsGameMaster
            };

            if (userData.Characters != null)
            {
                foreach (var characterName in userData.Characters)
                {
                    // This assumes a method to load character data by name exists
                    // user.Characters.Add(Character.LoadCharacterData(characterName));
                }
            }

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user {username}: {ex.Message}");
            return new User(username, "");
        }
    }

    public User Clone()
    {
        return new User
        {
            Id = Id,
            Name = Name,
            Username = Username,
            HashedPassword = HashedPassword,
            NCID = NCID,
            NCPassword = NCPassword,
            Email = Email,
            CreationDate = CreationDate,
            LastLoginDate = LastLoginDate,
            TotalPlayTime = TotalPlayTime,
            IsOnline = IsOnline,
            LastIPAddress = LastIPAddress,
            LoginCount = LoginCount,
            IsActive = IsActive,
            IsBanned = IsBanned,
            BanReason = BanReason,
            BanExpiry = BanExpiry,
            IsGameMaster = IsGameMaster,
            Characters = [..Characters],
            Inventory = [..Inventory],
            Level = Level,
            Experience = Experience,
            Health = Health,
            MaxHealth = MaxHealth,
            Mana = Mana,
            MaxMana = MaxMana,
            Gold = Gold,
            Gender = Gender,
            HairStyle = HairStyle,
            HairColor = HairColor,
            SkinColor = SkinColor,
            ClassName = ClassName,
            Path = Path,
            PathLevel = PathLevel,
            Strength = Strength,
            Intelligence = Intelligence,
            Wisdom = Wisdom,
            Constitution = Constitution,
            Dexterity = Dexterity,
            IsDirty = IsDirty,
            Friends = [..Friends],
            Ignored = [..Ignored],
            Guild = Guild,
            GuildRank = GuildRank,
            LastMap = LastMap,
            MapX = MapX,
            MapY = MapY,
            IsNewCharacter = IsNewCharacter
        };
    }
}