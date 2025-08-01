using System;
using System.Collections.Generic;
using System.Threading;

namespace DarkAges.Library.Core;

/// <summary>
/// Manages timed events and event scheduling for the game
/// </summary>
public class TimerEventMan : IDisposable
{
    private readonly List<TimerEvent> _events = [];
    private readonly List<TimerEvent> _toRemove = [];
    private readonly object _lockObject = new();
    private bool _isDisposed;

    public void AddEvent(string name, float interval, Action callback, bool repeating = false)
    {
        if (_isDisposed) return;
            
        lock (_lockObject)
        {
            _events.Add(new TimerEvent
            {
                Name = name,
                Interval = interval,
                Callback = callback,
                IsRepeating = repeating,
                TimeRemaining = interval,
                IsActive = true
            });
        }
    }

    public void RemoveEvent(string name)
    {
        if (_isDisposed) return;
            
        lock (_lockObject)
        {
            for (var i = _events.Count - 1; i >= 0; i--)
            {
                if (_events[i].Name == name)
                {
                    _events.RemoveAt(i);
                }
            }
        }
    }

    public void Update(float deltaTime)
    {
        if (_isDisposed) return;
            
        lock (_lockObject)
        {
            _toRemove.Clear();
                
            foreach (var evt in _events)
            {
                if (!evt.IsActive) continue;
                    
                evt.TimeRemaining -= deltaTime;
                    
                if (evt.TimeRemaining <= 0)
                {
                    try
                    {
                        evt.Callback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in timer event '{evt.Name}': {ex.Message}");
                    }
                        
                    if (evt.IsRepeating)
                    {
                        evt.TimeRemaining = evt.Interval;
                    }
                    else
                    {
                        _toRemove.Add(evt);
                    }
                }
            }
                
            foreach (var evt in _toRemove)
            {
                _events.Remove(evt);
            }
        }
    }

    public void Clear()
    {
        lock (_lockObject)
        {
            _events.Clear();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
            
        _isDisposed = true;
        Clear();
    }

    private class TimerEvent
    {
        public string Name { get; set; } = "";
        public float Interval { get; set; }
        public float TimeRemaining { get; set; }
        public Action? Callback { get; set; }
        public bool IsRepeating { get; set; }
        public bool IsActive { get; set; }
    }
}