using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingPlayPane : ControlPane
{
    private readonly List<RopeSkippingRope> _ropes = [];
    private readonly List<RopeSkippingMovableObject> _objects = [];
    private bool _jumpDetected = false;
    private DateTime _lastJumpTime = DateTime.Now;
    private int _ropeSpeed = 2;
    private int _objectSpeed = 3;

    public bool JumpDetected => _jumpDetected;

    public void Initialize()
    {
        _ropes.Clear();
        _objects.Clear();
        _jumpDetected = false;

        for (var i = 0; i < 3; i++)
        {
            var rope = new RopeSkippingRope
            {
                X = 100 + i * 200,
                Y = 0,
                Width = 100,
                Height = 400,
                Speed = _ropeSpeed
            };
            _ropes.Add(rope);
        }

        for (var i = 0; i < 2; i++)
        {
            var obj = new RopeSkippingMovableObject
            {
                X = 150 + i * 200,
                Y = 350,
                Width = 50,
                Height = 50,
                Speed = _objectSpeed
            };
            _objects.Add(obj);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        foreach (var rope in _ropes)
        {
            var rect = new Rectangle(rope.X, rope.Y, rope.Width, rope.Height);
            spriteBatch.DrawRectangle(rect, Color.Brown);
        }

        foreach (var obj in _objects)
        {
            var rect = new Rectangle(obj.X, obj.Y, obj.Width, obj.Height);
            spriteBatch.DrawRectangle(rect, Color.Blue);
        }

        var playerRect = new Rectangle(200, 300, 40, 60);
        spriteBatch.DrawRectangle(playerRect, Color.Red);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (e is KeyEvent ke && ke.Type == EventType.KeyDown)
        {
            if (ke.Key == Silk.NET.Input.Key.Space) // Spacebar
            {
                _jumpDetected = true;
                _lastJumpTime = DateTime.Now;
                return true;
            }
        }

        return base.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
        foreach (var rope in _ropes)
        {
            rope.Y += rope.Speed;
            if (rope.Y > 400)
            {
                rope.Y = -rope.Height;
            }
        }

        foreach (var obj in _objects)
        {
            obj.Y -= obj.Speed;
            if (obj.Y < -obj.Height)
            {
                obj.Y = 400;
            }
        }
    }

    public void ResetJumpDetection()
    {
        _jumpDetected = false;
    }

    public override void Dispose()
    {
        _ropes.Clear();
        _objects.Clear();
    }
}