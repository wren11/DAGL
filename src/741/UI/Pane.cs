using System;
using System.Drawing;
using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI;

public abstract class Pane : ControlPane
{
    public string Name { get; set; } = "";
    public bool IsModal { get; set; } = false;
    public bool IsDraggable { get; set; } = false;
    public bool IsResizable { get; set; } = false;

    protected Pane()
    {
    }

    protected Pane(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public virtual void OnShown() { }
    public virtual void OnClosed() { }
    public virtual void OnResize(Size newSize) { }
    public virtual void OnMove(Point newLocation) { }
}