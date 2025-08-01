namespace DarkAges.Library.GameLogic.Constraints;

public class ConstraintEventArgs(EventConstraint constraint) : EventArgs
{
    public EventConstraint Constraint { get; } = constraint ?? throw new ArgumentNullException(nameof(constraint));
}