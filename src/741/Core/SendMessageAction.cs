namespace DarkAges.Library.Core;

public class SendMessageAction(Action<string> sendMessage, string message) : MonitorAction
{
    public override void Execute()
    {
        if (_isDisposed) return;
        sendMessage?.Invoke(message);
    }
}