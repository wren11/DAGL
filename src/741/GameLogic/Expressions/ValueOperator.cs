namespace DarkAges.Library.GameLogic.Expressions;

public class ValueOperator<T>(T value) : Expression<T>
{
    public override T EvaluateTyped()
    {
        return value;
    }
}