using System.Collections.Concurrent;

namespace DarkAges.Library.Core.Events;

public class EventQueue
{
    private readonly ConcurrentQueue<Event> _eventQueue = new ConcurrentQueue<Event>();

    public void Enqueue(Event gameEvent)
    {
        _eventQueue.Enqueue(gameEvent);
    }

    public Event? Dequeue()
    {
        _eventQueue.TryDequeue(out var gameEvent);
        return gameEvent;
    }

    public bool TryDequeue(out Event? gameEvent)
    {
        return _eventQueue.TryDequeue(out gameEvent);
    }

    public bool IsEmpty => _eventQueue.IsEmpty;
}