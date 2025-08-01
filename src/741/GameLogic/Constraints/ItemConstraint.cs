namespace DarkAges.Library.GameLogic.Constraints;

public class ItemConstraint(int itemId, int quantity) : EventConstraint($"Item_{itemId}_{quantity}")
{
    public int RequiredItemId { get; set; } = itemId;
    public int RequiredQuantity { get; set; } = quantity;
    public int CurrentQuantity { get; set; }

    public override bool Evaluate()
    {
        return CurrentQuantity >= RequiredQuantity;
    }

    public void UpdateQuantity(int quantity)
    {
        CurrentQuantity = quantity;
    }
}