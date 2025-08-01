using System;
using System.Collections.Generic;
using DarkAges.Library.UI;

namespace DarkAges.Library.Core.Events;

public class EventManager
{
    private static EventManager? _instance;
    private readonly Dictionary<EventType, List<EventHandler<Event>>> _eventHandlers;
    private readonly Queue<Event> _eventQueue;
    private bool _isProcessingEvents;

    public static EventManager Instance
    {
        get
        {
            _instance ??= new EventManager();
            return _instance;
        }
    }

    private EventManager()
    {
        _eventHandlers = new Dictionary<EventType, List<EventHandler<Event>>>();
        _eventQueue = new Queue<Event>();
        _isProcessingEvents = false;
    }

    public void Subscribe(EventType eventType, EventHandler<Event> handler)
    {
        if (!_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] = [];
        }
        _eventHandlers[eventType].Add(handler);
    }

    public void Unsubscribe(EventType eventType, EventHandler<Event> handler)
    {
        if (_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType].Remove(handler);
        }
    }

    public void PostEvent(Event evt)
    {
        if (_isProcessingEvents)
        {
            _eventQueue.Enqueue(evt);
        }
        else
        {
            ProcessEvent(evt);
        }
    }

    private void ProcessEvent(Event evt)
    {
        if (_eventHandlers.TryGetValue(evt.Type, out var eventHandler))
        {
            foreach (var handler in eventHandler)
            {
                handler?.Invoke(this, evt);
            }
        }
    }

    public void DispatchEvent(Event evt)
    {
        PostEvent(evt);
    }

    public void Publish(Event evt)
    {
        _eventQueue.Enqueue(evt);
        if (!_isProcessingEvents)
        {
            ProcessEvents();
        }
    }

    public void ProcessEvents()
    {
        _isProcessingEvents = true;
        while (_eventQueue.Count > 0)
        {
            var evt = _eventQueue.Dequeue();
            if (_eventHandlers.ContainsKey(evt.Type))
            {
                foreach (var handler in _eventHandlers[evt.Type])
                {
                    handler?.Invoke(this, evt);
                }
            }
        }
        _isProcessingEvents = false;
    }

    public void Clear()
    {
        _eventHandlers.Clear();
        _eventQueue.Clear();
    }

    public void DispatchEvent(EventType custom, InventorySlotEvent eventData)
    {
        var customEvent = new CustomEvent(custom, eventData);
        PostEvent(customEvent);
    }
}