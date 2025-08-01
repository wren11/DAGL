namespace DarkAges.Library.GameLogic.Expressions;

public abstract class TernaryOperator<TResult, TFirst, TSecond, TThird>(
    Expression<TFirst> first,
    Expression<TSecond> second,
    Expression<TThird> third)
        : Expression<TResult>
{
    protected readonly Expression<TFirst> _first = first;
    protected readonly Expression<TSecond> _second = second;
    protected readonly Expression<TThird> _third = third;
}