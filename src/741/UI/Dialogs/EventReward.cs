namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Event reward information
/// </summary>
public class EventReward
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; } = 1;
    public RewardType Type { get; set; } = RewardType.Item;
    public int ItemId { get; set; }
    public int Experience { get; set; }
    public int Gold { get; set; }
}