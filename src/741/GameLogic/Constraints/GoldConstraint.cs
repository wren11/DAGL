namespace DarkAges.Library.GameLogic.Constraints;

public class GoldConstraint(int gold) : EventConstraint($"Gold_{gold}")
{
    public int RequiredGold { get; set; } = gold;
    public int CurrentGold { get; set; }

    public override bool Evaluate()
    {
        return CurrentGold >= RequiredGold;
    }

    public void UpdateGold(int gold)
    {
        CurrentGold = gold;
    }
}