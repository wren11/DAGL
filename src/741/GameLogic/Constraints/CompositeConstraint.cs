namespace DarkAges.Library.GameLogic.Constraints;

public class CompositeConstraint(string name, CompositeOperator op = CompositeOperator.And) : EventConstraint(name)
{
    private readonly List<EventConstraint> _constraints = [];
    public CompositeOperator Operator { get; set; } = op;

    public void AddConstraint(EventConstraint constraint)
    {
        if (constraint != null)
        {
            _constraints.Add(constraint);
        }
    }

    public void RemoveConstraint(EventConstraint constraint)
    {
        _constraints.Remove(constraint);
    }

    public override bool Evaluate()
    {
        if (_constraints.Count == 0)
            return true;

        switch (Operator)
        {
        case CompositeOperator.And:
            return _constraints.TrueForAll(c => c.Evaluate());
        case CompositeOperator.Or:
            return _constraints.Exists(c => c.Evaluate());
        case CompositeOperator.Not:
            return _constraints.Count == 1 && !_constraints[0].Evaluate();
        default:
            return true;
        }
    }

    public override void Reset()
    {
        base.Reset();
        foreach (var constraint in _constraints)
        {
            constraint.Reset();
        }
    }
}