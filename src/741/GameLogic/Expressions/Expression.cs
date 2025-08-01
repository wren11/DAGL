namespace DarkAges.Library.GameLogic.Expressions;

public abstract class Expression
{
    public abstract object Evaluate();
}

public abstract class Expression<T> : Expression
{
    public override object Evaluate()
    {
        return EvaluateTyped();
    }

    public abstract T EvaluateTyped();
}