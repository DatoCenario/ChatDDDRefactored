using System.Linq.Expressions;

namespace Chatiks.Tools.Extensions.Models;

public class SubstituteVisitor: ExpressionVisitor
{
    private readonly Dictionary<string, Expression> _substives;

    public SubstituteVisitor(Dictionary<string, Expression> substives)
    {
        _substives = substives;
    }

    public override Expression? Visit(Expression? node)
    {
        if (node is MethodCallExpression call && call.Object is NewExpression subInstance)
        {
            if (subInstance.Arguments[0] is ConstantExpression arg)
            {
                var name = arg.Value as string;

                if (_substives.TryGetValue(name, out var sub))
                {
                    return base.Visit(sub);
                }
            }
        }
        
        return base.Visit(node);
    }
}