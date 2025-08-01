namespace DarkAges.Library.GameLogic;

public class ItemEffect : ICloneable
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ItemEffectType Type { get; set; }
    public int Value { get; set; }
    public float Duration { get; set; }
    public bool IsPermanent { get; set; } = true;
    public bool IsStackable { get; set; } = false;
    public int MaxStacks { get; set; } = 1;
    public string IconPath { get; set; } = "";
    public Dictionary<string, object> Parameters { get; set; } = new();

    public void Apply(World.WorldObject_Human user)
    {
        // Apply the effect to the user
    }

    public ItemEffect Clone()
    {
        var cloned = new ItemEffect
        {
            Name = Name,
            Description = Description,
            Type = Type,
            Value = Value,
            Duration = Duration,
            IsPermanent = IsPermanent,
            IsStackable = IsStackable,
            MaxStacks = MaxStacks,
            IconPath = IconPath
        };

        foreach (var param in Parameters)
            cloned.Parameters[param.Key] = param.Value;

        return cloned;
    }

    object ICloneable.Clone() => Clone();

    public override string ToString()
    {
        return $"{Name}: {Description}";
    }
}