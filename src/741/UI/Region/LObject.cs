using System.Drawing;
using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Region;

public abstract class LObject(int id, string name)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));
    public Point Position { get; set; }
    public Size Size { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;

    public virtual void Update()
    {
    }

    public virtual void Render()
    {
    }

    public virtual bool HandleEvent(Event e)
    {
        return false;
    }

    public virtual void Dispose()
    {
    }
}