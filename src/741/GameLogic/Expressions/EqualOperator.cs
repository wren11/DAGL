namespace DarkAges.Library.GameLogic.Expressions;

public class EqualOperator<T>(Expression<T> left, Expression<T> right) : BinaryOperator<bool, T, T>(left, right)
{

    public override bool EvaluateTyped()
    {
        var leftVal = _left.EvaluateTyped();
        var rightVal = _right.EvaluateTyped();
        return leftVal.Equals(rightVal);
    }
}