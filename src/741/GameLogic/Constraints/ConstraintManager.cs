namespace DarkAges.Library.GameLogic.Constraints;

public class ConstraintManager
{
    private readonly List<EventConstraint> _constraints = [];
    private readonly Dictionary<string, EventConstraint> _constraintMap = new Dictionary<string, EventConstraint>();

    public event EventHandler<ConstraintEventArgs> ConstraintSatisfied;
    public event EventHandler<ConstraintEventArgs> ConstraintFailed;

    public void AddConstraint(EventConstraint constraint)
    {
        if (constraint == null)
            return;

        _constraints.Add(constraint);
        _constraintMap[constraint.Name] = constraint;

        constraint.ConstraintSatisfied += (s, e) => ConstraintSatisfied?.Invoke(this, e);
        constraint.ConstraintFailed += (s, e) => ConstraintFailed?.Invoke(this, e);
    }

    public void RemoveConstraint(EventConstraint constraint)
    {
        if (constraint == null)
            return;

        _constraints.Remove(constraint);
        _constraintMap.Remove(constraint.Name);
    }

    public EventConstraint GetConstraint(string name)
    {
        return _constraintMap.TryGetValue(name, out var constraint) ? constraint : null;
    }

    public bool CheckAllConstraints()
    {
        var allSatisfied = true;
        foreach (var constraint in _constraints)
        {
            if (!constraint.Check())
            {
                allSatisfied = false;
            }
        }
        return allSatisfied;
    }

    public void ResetAllConstraints()
    {
        foreach (var constraint in _constraints)
        {
            constraint.Reset();
        }
    }

    public void EnableConstraint(string name, bool enabled)
    {
        var constraint = GetConstraint(name);
        if (constraint != null)
        {
            constraint.IsEnabled = enabled;
        }
    }

    public void SetCheckInterval(string name, int interval)
    {
        var constraint = GetConstraint(name);
        if (constraint != null)
        {
            constraint.CheckInterval = interval;
        }
    }
}