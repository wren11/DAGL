namespace DarkAges.Library.GameLogic.Constraints;

public class SpellSkillConstraint(int spellId, int skillLevel) : EventConstraint($"SpellSkill_{spellId}_{skillLevel}")
{
    public int RequiredSpellId { get; set; } = spellId;
    public int RequiredSkillLevel { get; set; } = skillLevel;
    public int CurrentSpellLevel { get; set; }
    public int CurrentSkillLevel { get; set; }

    public override bool Evaluate()
    {
        return CurrentSpellLevel >= RequiredSpellId && CurrentSkillLevel >= RequiredSkillLevel;
    }

    public void UpdateLevels(int spellLevel, int skillLevel)
    {
        CurrentSpellLevel = spellLevel;
        CurrentSkillLevel = skillLevel;
    }
}