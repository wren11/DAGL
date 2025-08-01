using System;
using System.Collections.Generic;

namespace DarkAges.Library.Core.Events;

public class EventDispatcher
{
    private readonly Queue<Event> _eventQueue = new Queue<Event>();
    private readonly List<IEventHandler> _handlers = [];

    public void PostEvent(Event ev)
    {
        _eventQueue.Enqueue(ev);
    }

    public void AddHandler(IEventHandler handler)
    {
        if (!_handlers.Contains(handler))
        {
            _handlers.Add(handler);
        }
    }

    public void RemoveHandler(IEventHandler handler)
    {
        _handlers.Remove(handler);
    }

    public void ProcessEvents()
    {
        while (_eventQueue.Count > 0)
        {
            var ev = _eventQueue.Dequeue();
                
            foreach (var handler in _handlers)
            {
                if (handler.HandleEvent(ev))
                {
                    break; // Event was handled, stop processing
                }
            }
        }
    }

    public void Clear()
    {
        _eventQueue.Clear();
    }
}