namespace DarkAges.Library.GameLogic.Expressions;

public abstract class UnaryOperator<TResult, TInput>(Expression<TInput> operand) : Expression<TResult>
{
    protected readonly Expression<TInput> _operand = operand;
}