namespace DarkAges.Library.UI.NPC;

public class MerchantItemEventArgs(MerchantItem item) : EventArgs
{
    public MerchantItem Item { get; } = item ?? throw new ArgumentNullException(nameof(item));
}