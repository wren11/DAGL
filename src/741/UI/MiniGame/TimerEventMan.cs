namespace DarkAges.Library.UI.MiniGame;

public class TimerEventMan
{
    private readonly List<TimerEvent> _events = [];
    private readonly List<TimerEvent> _pendingEvents = [];

    public void AddEvent(TimerEvent timerEvent)
    {
        _pendingEvents.Add(timerEvent);
    }

    public void RemoveEvent(TimerEvent timerEvent)
    {
        _events.Remove(timerEvent);
        _pendingEvents.Remove(timerEvent);
    }

    public void Update(float deltaTime)
    {
        var now = DateTime.Now;

        foreach (var evt in _pendingEvents)
        {
            _events.Add(evt);
        }
        _pendingEvents.Clear();

        for (var i = _events.Count - 1; i >= 0; i--)
        {
            var evt = _events[i];
            if (now >= evt.TriggerTime)
            {
                evt.Execute();
                if (evt.IsOneTime)
                {
                    _events.RemoveAt(i);
                }
                else
                {
                    evt.TriggerTime = now.Add(evt.Interval);
                }
            }
        }
    }

    public void Dispose()
    {
        _events.Clear();
        _pendingEvents.Clear();
    }
}