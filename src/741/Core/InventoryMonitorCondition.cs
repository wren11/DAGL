namespace DarkAges.Library.Core;

public class InventoryMonitorCondition(Func<int> getItemCount, int itemId, int threshold = 1) : MonitorCondition
{
    private readonly int _itemId = itemId;

    public override bool Evaluate()
    {
        if (_isDisposed) return false;

        var itemCount = getItemCount();
        return itemCount <= threshold;
    }
}