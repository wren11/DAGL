using System.Drawing;

namespace DarkAges.Library.UI;

/// <summary>
/// Mock spell database for demonstration.
/// </summary>
public static class SpellDatabase
{
    public static SpellData GetSpell(int spellId)
    {
        // Mock implementation - in practice, this would query a real database
        return new SpellData
        {
            Id = spellId,
            Name = $"Spell {spellId}",
            Description = "A powerful spell that does magical damage.",
            SubDescriptions = new string[5],
            SubDescriptionFlags = new int[5],
            Positions = new Position[5],
            Colors = new Color[5]
        };
    }
}