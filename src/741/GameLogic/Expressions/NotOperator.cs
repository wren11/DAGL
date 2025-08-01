namespace DarkAges.Library.GameLogic.Expressions;

public class NotOperator(Expression<bool> operand) : UnaryOperator<bool, bool>(operand)
{
    public override bool EvaluateTyped()
    {
        return !_operand.EvaluateTyped();
    }
}