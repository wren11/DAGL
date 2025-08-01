using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DarkAges.Library.GameLogic.Expressions;

public class ExpressionParser
{
    private readonly Dictionary<string, Expression> _variables = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, OperatorInfo> _operators = new()
    {
        // From sub_46CF80 and sub_46D7C0
        ["?"] = new OperatorInfo(1, true, 3), // Ternary has low precedence
        [":"] = new OperatorInfo(1, true, 3),
        ["||"] = new OperatorInfo(2, false, 2),
        ["&&"] = new OperatorInfo(3, false, 2),
        ["=="] = new OperatorInfo(4, false, 2),
        ["!="] = new OperatorInfo(4, false, 2),
        ["<"] = new OperatorInfo(5, false, 2),
        [">"] = new OperatorInfo(5, false, 2),
        ["<="] = new OperatorInfo(5, false, 2),
        [">="] = new OperatorInfo(5, false, 2),
        ["+"] = new OperatorInfo(6, false, 2),
        ["-"] = new OperatorInfo(6, false, 2),
        ["*"] = new OperatorInfo(7, false, 2),
        ["/"] = new OperatorInfo(7, false, 2),
        ["!"] = new OperatorInfo(8, true, 1) // Unary not
    };

    private class OperatorInfo(int precedence, bool isRightAssociative, int arity)
    {
        public int Precedence { get; } = precedence;
        public bool IsRightAssociative { get; } = isRightAssociative;
        public int Arity { get; } = arity;
    }

    // From sub_46CF80 and sub_46D7C0
    // Ternary has low precedence
    // Unary not

    private IEnumerable<Tuple<string, TokenType>> Tokenize(string expression)
    {
        var tokenRegex = new Regex(@"(\d+\.?\d*|\d*\.?\d+)|([a-zA-Z_][a-zA-Z0-9_]*)|(==|!=|<=|>=|&&|\|\||.)");
        var matches = tokenRegex.Matches(expression);
        foreach (Match match in matches)
        {
            var value = match.Value;
            if (string.IsNullOrWhiteSpace(value)) continue;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                yield return Tuple.Create(value, TokenType.Number);
            else if (_operators.ContainsKey(value) || value == "(" || value == ")")
                yield return Tuple.Create(value, TokenType.Operator);
            else
                yield return Tuple.Create(value, TokenType.Identifier);
        }
    }

    private enum TokenType
    {
        Number,
        Operator,
        Identifier
    }

    public Expression Parse(string expression)
    {
        var tokens = Tokenize(expression);
        var outputQueue = new Queue<Tuple<string, TokenType>>();
        var operatorStack = new Stack<Tuple<string, TokenType>>();

        foreach (var token in tokens)
        {
            switch (token.Item2)
            {
            case TokenType.Number:
            case TokenType.Identifier:
                outputQueue.Enqueue(token);
                break;
            case TokenType.Operator:
                if (token.Item1 == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token.Item1 == ")")
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek().Item1 != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }

                    if (operatorStack.Count > 0 && operatorStack.Peek().Item1 == "(")
                    {
                        operatorStack.Pop();
                    }
                    else
                    {
                        throw new ArgumentException("Mismatched parentheses.");
                    }
                }
                else
                {
                    var op1Info = _operators[token.Item1];
                    while (operatorStack.Count > 0 && _operators.ContainsKey(operatorStack.Peek().Item1))
                    {
                        var op2Info = _operators[operatorStack.Peek().Item1];
                        if ((!op1Info.IsRightAssociative && op1Info.Precedence <= op2Info.Precedence) ||
                            (op1Info.IsRightAssociative && op1Info.Precedence < op2Info.Precedence))
                        {
                            outputQueue.Enqueue(operatorStack.Pop());
                        }
                        else
                        {
                            break;
                        }
                    }

                    operatorStack.Push(token);
                }

                break;
            }
        }

        while (operatorStack.Count > 0)
        {
            var op = operatorStack.Pop();
            if (op.Item1 == "(")
                throw new ArgumentException("Mismatched parentheses.");
            outputQueue.Enqueue(op);
        }

        return BuildExpressionTree(outputQueue);
    }

    private Expression BuildExpressionTree(Queue<Tuple<string, TokenType>> rpnQueue)
    {
        var stack = new Stack<Expression>();
        while (rpnQueue.Count > 0)
        {
            var token = rpnQueue.Dequeue();
            if (token.Item2 == TokenType.Number)
            {
                stack.Push(new ValueOperator<double>(double.Parse(token.Item1, CultureInfo.InvariantCulture)));
            }
            else if (token.Item2 == TokenType.Identifier)
            {
                if (_variables.TryGetValue(token.Item1, out var variable))
                {
                    stack.Push(variable);
                }
                else
                {
                    throw new ArgumentException($"Undefined variable: {token.Item1}");
                }
            }
            else if (token.Item2 == TokenType.Operator)
            {
                var opInfo = _operators[token.Item1];
                if (opInfo.Arity == 1) // Unary operator
                {
                    var operand = stack.Pop();
                    stack.Push(CreateUnaryOperator(token.Item1, operand));
                }
                else // Binary operator
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    stack.Push(CreateBinaryOperator(token.Item1, left, right));
                }
            }
        }
        if (stack.Count != 1)
        {
            throw new ArgumentException("Invalid expression.");
        }
        return stack.Pop();
    }

private Expression CreateUnaryOperator(string op, Expression operand)
    {
        switch (op)
        {
        case "!": return new NotOperator((Expression<bool>)operand);
        default: throw new ArgumentException($"Unknown unary operator: {op}");
        }
    }
        
    private Expression CreateBinaryOperator(string op, Expression left, Expression right)
    {
        switch (op)
        {
        case "+": return new AddOperator((Expression<double>)left, (Expression<double>)right);
        case "-": return new SubtractOperator((Expression<double>)left, (Expression<double>)right);
        case "*": return new MultiplyOperator((Expression<double>)left, (Expression<double>)right);
        case "/": return new DivideOperator((Expression<double>)left, (Expression<double>)right);
        case "<": return new LessOperator((Expression<double>)left, (Expression<double>)right);
        case ">": return new GreaterOperator((Expression<double>)left, (Expression<double>)right);
        case "<=": return new NotOperator(new GreaterOperator((Expression<double>)left, (Expression<double>)right));
        case ">=": return new NotOperator(new LessOperator((Expression<double>)left, (Expression<double>)right));
        case "&&": return new AndOperator((Expression<bool>)left, (Expression<bool>)right);
        case "||": return new OrOperator((Expression<bool>)left, (Expression<bool>)right);
        default: throw new ArgumentException($"Unknown binary operator: {op}");
        }
    }

    public T Evaluate<T>(string expression)
    {
        var expr = Parse(expression);
        if (expr is Expression<T> typedExpr)
        {
            return typedExpr.EvaluateTyped();
        }
            
        var result = expr.Evaluate();
        if (result is T convertedResult)
        {
            return convertedResult;
        }
            
        return (T)Convert.ChangeType(result, typeof(T), CultureInfo.InvariantCulture);
    }

    public void SetVariable(string name, double value)
    {
        _variables[name] = new ValueOperator<double>(value);
    }

    public void SetVariable(string name, bool value)
    {
        _variables[name] = new ValueOperator<bool>(value);
    }
}