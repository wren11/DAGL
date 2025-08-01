namespace DarkAges.Library.GameLogic.Expressions;

public class MultiplyOperator(Expression<double> left, Expression<double> right)
        : BinaryOperator<double, double, double>(left, right)
{
    public override double EvaluateTyped()
    {
        return _left.EvaluateTyped() * _right.EvaluateTyped();
    }
}