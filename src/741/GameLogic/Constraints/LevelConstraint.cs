namespace DarkAges.Library.GameLogic.Constraints;

public class LevelConstraint(int level) : EventConstraint($"Level_{level}")
{
    public int RequiredLevel { get; set; } = level;
    public int CurrentLevel { get; set; }

    public override bool Evaluate()
    {
        return CurrentLevel >= RequiredLevel;
    }

    public void UpdateLevel(int level)
    {
        CurrentLevel = level;
    }
}