using System.Linq.Expressions;

namespace Chatiks.Tools.Extensions.Visitors;

class ReplacerVisitor : ExpressionVisitor
{
    private readonly Dictionary<Expression, Expression> _replaces;
    private readonly  Func<Expression, Expression> _customReplace;

    public ReplacerVisitor(
        Dictionary<Expression, Expression> replaces = null,
        Func<Expression, Expression> customReplace = null)
    {
        _replaces = replaces;
        _customReplace = customReplace;
    }

    public override Expression Visit(Expression? node)
    {
        if (node == null)
        {
            return base.Visit(node);
        }

        var newNode = _customReplace?.Invoke(node);
        if (newNode != null)
        {
            return base.Visit(newNode);
        }
            
        if (_replaces.TryGetValue(node, out var replace))
        {
            return base.Visit(replace);
        }

        return base.Visit(node);
    }
}