using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;

namespace DarkAges.Library.GameLogic.Constraints;

public abstract class EventConstraint(string name)
{
    public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));
    public bool IsEnabled { get; set; } = true;
    public DateTime LastCheck { get; set; }
    public int CheckInterval { get; set; } = 1000; // milliseconds

    public event EventHandler<ConstraintEventArgs> ConstraintSatisfied;
    public event EventHandler<ConstraintEventArgs> ConstraintFailed;

    public abstract bool Evaluate();

    public virtual bool Check()
    {
        if (!IsEnabled)
            return true;

        var now = DateTime.Now;
        if ((now - LastCheck).TotalMilliseconds < CheckInterval)
            return true;

        LastCheck = now;
        var result = Evaluate();

        if (result)
        {
            ConstraintSatisfied?.Invoke(this, new ConstraintEventArgs(this));
        }
        else
        {
            ConstraintFailed?.Invoke(this, new ConstraintEventArgs(this));
        }

        return result;
    }

    public virtual void Reset()
    {
        LastCheck = DateTime.MinValue;
    }
}