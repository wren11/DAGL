namespace DarkAges.Library.UI.MiniGame;

public class TimerEvent(TimeSpan interval, Action action, bool isOneTime = false)
{
    public DateTime TriggerTime { get; set; } = DateTime.Now.Add(interval);
    public TimeSpan Interval { get; set; } = interval;
    public bool IsOneTime { get; set; } = isOneTime;
    public Action Action { get; set; } = action;

    public void Execute()
    {
        Action?.Invoke();
    }
}