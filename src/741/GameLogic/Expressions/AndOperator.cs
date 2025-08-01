namespace DarkAges.Library.GameLogic.Expressions;

public class AndOperator(Expression<bool> left, Expression<bool> right)
        : BinaryOperator<bool, bool, bool>(left, right)
{
    public override bool EvaluateTyped()
    {
        return _left.EvaluateTyped() && _right.EvaluateTyped();
    }
}