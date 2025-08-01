namespace DarkAges.Library.GameLogic.Expressions;

public class LessOperator(Expression<double> left, Expression<double> right)
        : BinaryOperator<bool, double, double>(left, right)
{
    public override bool EvaluateTyped()
    {
        return _left.EvaluateTyped() < _right.EvaluateTyped();
    }
}