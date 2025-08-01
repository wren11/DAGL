namespace DarkAges.Library.GameLogic.Expressions;

public abstract class BinaryOperator<TResult, TLeft, TRight>(Expression<TLeft> left, Expression<TRight> right)
        : Expression<TResult>
{
    protected readonly Expression<TLeft> _left = left;
    protected readonly Expression<TRight> _right = right;
}