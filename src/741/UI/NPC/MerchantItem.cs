namespace DarkAges.Library.UI.NPC;

public class MerchantItem(int id, string name, int price, int quantity = -1)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));
    public int Price { get; set; } = price;
    public int Quantity { get; set; } = quantity;
    public string? Description { get; set; }
    public ItemType Type { get; set; }
    public bool IsStackable { get; set; } = true;
    public int MaxStack { get; set; } = 99;
}