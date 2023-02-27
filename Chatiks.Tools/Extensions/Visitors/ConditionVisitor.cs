using System.Linq.Expressions;

namespace Chatiks.Tools.Extensions.Visitors;

public class ConditionVisitor : ExpressionVisitor
{
    private readonly Func<Expression, bool> _cond;
    private List<Expression> _found;

    public ConditionVisitor(Func<Expression, bool> cond)
    {
        _cond = cond;
    }

    public override Expression? Visit(Expression? node)
    {
        if (_cond.Invoke(node))
        {
            _found.Add(node);
        }
            
        return base.Visit(node);
    }

    public List<Expression> Find(Expression exp)
    {
        _found = new List<Expression>();
        Visit(exp);
        return _found;
    }
}