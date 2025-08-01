using System.Drawing;

namespace DarkAges.Library.UI;

/// <summary>
/// Spell data structure.
/// </summary>
public class SpellData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] SubDescriptions { get; set; } = [];
    public int[] SubDescriptionFlags { get; set; } = [];
    public Position[] Positions { get; set; } = [];
    public Color[] Colors { get; set; } = [];
}