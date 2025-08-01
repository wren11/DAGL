namespace DarkAges.Library.GameLogic.Expressions;

//TODO fix

/*
public class ConditionalOperator<T>(Expression<bool> condition, Expression ifTrue, Expression<T> ifFalse)
        : TernaryOperator<T, bool, T, T>(condition, ifFalse)
{
    public ConditionalOperator(Expression<bool> expression, Expression ifTrue1, Expression ifFalse1) : base(expression)
    {
        throw new NotImplementedException();
    }

    public override T EvaluateTyped()
    {
        return _first.EvaluateTyped() ? _second.EvaluateTyped() : _third.EvaluateTyped();
    }
}
*/

public class Conditions
{
    public static ConditionalOperator<T> If<T>(Expression<bool> condition, Expression<T> ifTrue, Expression<T> ifFalse)
    {
        return new ConditionalOperator<T>(condition, ifTrue, ifFalse);
    }

}

public class ConditionalOperator<T>(Expression<bool> condition, Expression<object> ifTrue, Expression<object> ifFalse)
{
    private readonly Expression<bool> _condition = condition;
    private readonly Expression<object> _ifTrue = ifTrue;
    private readonly Expression<object> _ifFalse = ifFalse;

    public ConditionalOperator(Expression<bool> expression, Expression ifTrue1, Expression ifFalse1) : this(expression, (Expression<object>)ifTrue1, (Expression<object>)ifFalse1)
    {
        _condition = expression;
        _ifTrue = (Expression<object>)ifTrue1;
        _ifFalse = (Expression<object>)ifFalse1;
    }
}