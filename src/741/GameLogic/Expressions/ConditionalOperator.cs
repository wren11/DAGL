namespace DarkAges.Library.GameLogic.Expressions;

public class ConditionalOperator<T> : TernaryOperator<T, bool, T, T>
{
    public ConditionalOperator(Expression<bool> condition, Expression<T> ifTrue, Expression<T> ifFalse)
        : base(condition, ifTrue, ifFalse)
    {
    }

    public override T EvaluateTyped()
    {
        return _first.EvaluateTyped() ? _second.EvaluateTyped() : _third.EvaluateTyped();
    }
}