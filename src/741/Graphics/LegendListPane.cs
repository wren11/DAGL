using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.Graphics;

/// <summary>
/// A UI component for displaying a list of legends.
/// </summary>
public class LegendListPane
{
    private const int MaxEntries = 8;
    public List<LegendEntry> Entries { get; } = new(MaxEntries);

    public void AddEntry(LegendEntry entry)
    {
        if (Entries.Count < MaxEntries)
        {
            Entries.Add(entry);
        }
    }

    /// <summary>
    /// Renders the legend list to a given surface.
    /// </summary>
    /// <param name="targetSurface">The surface to draw the legend list on.</param>
    /// <param name="x">The x-coordinate to start drawing at.</param>
    /// <param name="y">The y-coordinate to start drawing at.</param>
    public void Render(Surface targetSurface, int x, int y)
    {
        var currentY = y;
        foreach (var entry in Entries)
        {
            // For now, just draw the text directly to the surface
            // In a real implementation, you would use proper font rendering
            targetSurface.DrawText(entry.Text, new Point(x, currentY), entry.Color);
            currentY += 16; // Move to the next line
        }
    }
}