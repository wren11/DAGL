using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Screen;

public class ScreenPane : IDisposable
{
    private readonly List<ControlPane> _children = [];
    public bool IsVisible { get; set; } = true;
        
    public virtual void Update(double deltaTime)
    {
        // Update logic for the screen pane and its children
    }

    public virtual void Render()
    {
        // Rendering logic for the screen pane and its children
    }
        
    public void AddChild(ControlPane child)
    {
        _children.Add(child);
    }

    public void RemoveChild(ControlPane child)
    {
        _children.Remove(child);
    }

    public virtual void Dispose()
    {
        // Dispose logic
    }
}