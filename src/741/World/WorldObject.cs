using DarkAges.Library.Graphics;
using System.Drawing;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.World;

/// <summary>
/// Base class for all objects that exist in the game world
/// </summary>
public class WorldObject : IDisposable
{
    private static int _nextId = 1;
        
    public int ID { get; }
    public string Name { get; set; } = "";
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public float Rotation { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public bool IsVisible { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsCollidable { get; set; } = true;
    public bool IsDisposed { get; private set; }

    // Visual properties
    public IndexedImage? Sprite { get; set; }
    public Palette? Palette { get; set; }
    public Rectangle Bounds { get; set; }
    public Rectangle BoundingBox { get; set; }
    public int ZOrder { get; set; }
    public float Opacity { get; set; } = 1.0f;

    // Game properties
    public int MapId { get; set; }
    public byte Direction { get; set; } = 1; // 1-8 for isometric directions
    public WorldObjectType ObjectType { get; protected set; }
    public WorldObjectState State { get; set; } = WorldObjectState.Idle;

    // Events
    public event EventHandler<WorldObject>? ObjectMoved;
    public event EventHandler<WorldObject>? ObjectDestroyed;
    public event EventHandler<WorldObjectCollisionEventArgs>? ObjectCollided;

    public WorldObject()
    {
        ID = _nextId++;
    }

    public WorldObject(WorldObjectType objectType) : this()
    {
        ObjectType = objectType;
    }

    public void MoveTo(System.Numerics.Vector2 position)
    {
        Position = new Vector2(position.X, position.Y);
    }

    public void SetState(WorldObjectState state)
    {
        State = state;
    }

    public float GetDistanceTo(WorldObject other)
    {
        return Vector2.Distance(Position, other.Position);
    }

    public virtual void Update(float deltaTime)
    {
        if (!IsActive || IsDisposed) return;

        // Update position based on velocity
        if (Velocity != Vector2.Zero)
        {
            var oldPosition = Position;
            Position += Velocity * deltaTime;
            OnObjectMoved(oldPosition);
        }
    }

    public virtual void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || IsDisposed || spriteBatch == null || Sprite == null) return;

        spriteBatch.Draw(
            Sprite,
            Position,
            null,
            Palette ?? new Palette(),
            Rotation,
            Vector2.Zero,
            Scale,
            SpriteEffects.None,
            ZOrder,
            Opacity);
    }

    public virtual bool Intersects(WorldObject other)
    {
        if (other == null || !IsCollidable || !other.IsCollidable) return false;

        var thisRect = new Rectangle(
            (int)Position.X + Bounds.X,
            (int)Position.Y + Bounds.Y,
            Bounds.Width,
            Bounds.Height);

        var otherRect = new Rectangle(
            (int)other.Position.X + other.Bounds.X,
            (int)other.Position.Y + other.Bounds.Y,
            other.Bounds.Width,
            other.Bounds.Height);

        return thisRect.IntersectsWith(otherRect);
    }

    protected virtual void OnObjectMoved(Vector2 oldPosition)
    {
        ObjectMoved?.Invoke(this, this);
    }

    protected virtual void OnObjectDestroyed()
    {
        ObjectDestroyed?.Invoke(this, this);
    }

    protected virtual void OnObjectCollided(WorldObject other)
    {
        if (other == null) return;
        ObjectCollided?.Invoke(this, new WorldObjectCollisionEventArgs(this, other));
    }

    public virtual void Dispose()
    {
        if (IsDisposed) return;

        // Clean up resources
        Sprite?.Dispose();
        Palette?.Dispose();

        // Clear event handlers
        ObjectMoved = null;
        ObjectDestroyed = null;
        ObjectCollided = null;

        IsDisposed = true;
        OnObjectDestroyed();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((WorldObject)obj);
    }

    public virtual bool Equals(WorldObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ID == other.ID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public static bool operator ==(WorldObject? left, WorldObject? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(WorldObject? left, WorldObject? right)
    {
        return !(left == right);
    }
}