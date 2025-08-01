namespace DarkAges.Library.Core;

public class UseItemAction(Action<int> useItem, int itemId) : MonitorAction
{
    public override void Execute()
    {
        if (_isDisposed) return;
        useItem?.Invoke(itemId);
    }
}