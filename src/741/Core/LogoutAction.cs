namespace DarkAges.Library.Core;

public class LogoutAction(Action logout) : MonitorAction
{
    public override void Execute()
    {
        if (_isDisposed) return;
        logout?.Invoke();
    }
}