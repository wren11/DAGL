using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI;

public abstract class ControlPane
{
    public ControlPane Parent { get; private set; }
    public List<ControlPane> Children { get; } = [];
    public Rectangle Bounds { get; set; }
    public Rectangle ClientBounds => new(0, 0, Bounds.Width, Bounds.Height);
    public Point Location { get => Bounds.Location; set => SetPosition(value.X, value.Y); }
    public Point Position { get => Bounds.Location; set => SetPosition(value.X, value.Y); }
    public int X { get => Bounds.X; set => SetPosition(value, Y); }
    public int Y { get => Bounds.Y; set => SetPosition(X, value); }
    public System.Drawing.Size Size { get => Bounds.Size; set => SetSize(value.Width, value.Height); }
    public int Width { get => Bounds.Width; set => SetSize(value, Height); }
    public int Height { get => Bounds.Height; set => SetSize(Width, value); }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool Enabled { get => IsEnabled; set => IsEnabled = value; }
    public bool CanFocus { get; set; } = false;
    public bool HasFocus { get; protected set; } = false;

    protected ControlPane()
    {
    }

    protected ControlPane(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public virtual void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        foreach (var child in Children)
        {
            child.Render(spriteBatch);
        }
    }

    public virtual bool HandleEvent(Event e)
    {
        if (!IsEnabled) return false;

        foreach (var child in Children.Reverse<ControlPane>())
        {
            if (child.HandleEvent(e))
                return true;
        }

        return false;
    }

    public virtual bool HandleInput(Event e)
    {
        if (!IsEnabled) return false;

        foreach (var child in Children.Reverse<ControlPane>())
        {
            if (child.HandleInput(e))
                return true;
        }

        return false;
    }

    public void AddChild(ControlPane child)
    {
        Children.Add(child);
        child.SetParent(this);
    }

    public void RemoveChild(ControlPane child)
    {
        Children.Remove(child);
        child.SetParent(null);
    }

    public void SetParent(ControlPane parent)
    {
        Parent = parent;
    }

    public void SetPosition(int x, int y)
    {
        Bounds = new Rectangle(x, y, Bounds.Width, Bounds.Height);
    }

    public void SetSize(int width, int height)
    {
        Bounds = new Rectangle(Bounds.X, Bounds.Y, width, height);
    }

    public void SetBounds(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public void CenterOnScreen()
    {
        // Assuming a 640x480 screen for now
        var screenWidth = 640;
        var screenHeight = 480;

        var x = (screenWidth - Width) / 2;
        var y = (screenHeight - Height) / 2;

        SetPosition(x, y);
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
    }

    public void Show()
    {
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public virtual void SetFocus(bool focus)
    {
        if (CanFocus)
        {
            HasFocus = focus;
        }
    }

    public bool IsPointInBounds(int x, int y)
    {
        return Bounds.Contains(x, y);
    }

    public virtual void Update(float deltaTime)
    {
        foreach (var child in Children)
        {
            child.Update(deltaTime);
        }
    }

    public virtual void Dispose()
    {
        foreach (var child in Children)
        {
            child.Dispose();
        }
    }
}