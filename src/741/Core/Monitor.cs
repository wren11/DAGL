using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using System.Drawing;

namespace DarkAges.Library.Core;

public class Monitor : IDisposable
{
    private readonly List<MonitorCondition> _conditions = [];
    private readonly List<MonitorAction> _actions = [];
    private bool _isEnabled = true;
    private bool _isDisposed;

    public bool IsEnabled => _isEnabled;
    public int ConditionCount => _conditions.Count;
    public int ActionCount => _actions.Count;

    public void AddCondition(MonitorCondition condition)
    {
        if (_isDisposed) return;
        _conditions.Add(condition);
    }

    public void AddAction(MonitorAction action)
    {
        if (_isDisposed) return;
        _actions.Add(action);
    }

    public void Enable()
    {
        _isEnabled = true;
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    public void Update()
    {
        if (_isDisposed || !_isEnabled) return;

        var allConditionsMet = true;
        foreach (var condition in _conditions)
        {
            if (!condition.Evaluate())
            {
                allConditionsMet = false;
                break;
            }
        }

        if (allConditionsMet)
        {
            ExecuteActions();
        }
    }

    private void ExecuteActions()
    {
        foreach (var action in _actions)
        {
            action.Execute();
        }
    }

    public void Clear()
    {
        _conditions.Clear();
        _actions.Clear();
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        Clear();
        _isDisposed = true;
    }
}