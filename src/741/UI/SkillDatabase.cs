using System.Drawing;

namespace DarkAges.Library.UI;

/// <summary>
/// Mock skill database for demonstration.
/// </summary>
public static class SkillDatabase
{
    public static SkillData GetSkill(int skillId)
    {
        // Mock implementation - in practice, this would query a real database
        return new SkillData
        {
            Id = skillId,
            Name = $"Skill {skillId}",
            Description = "A useful skill that provides various benefits.",
            SubDescriptions = new string[5],
            SubDescriptionFlags = new int[5],
            Positions = new Position[5],
            Colors = new Color[5]
        };
    }
}