using System.Linq.Expressions;
using Chatiks.Tools.Extensions.Visitors;

namespace Chatiks.Tools.Extensions.Models;

public static class ChainExpressionFactory
{
    public static ChainExpression<TEIn, TEIn, TEOut> FromLambda<TEIn, TEOut>(
        Expression<Func<TEIn, TEOut>> lam)
    {
        return new ChainExpression<TEIn, TEIn, TEOut>(lam);
    }
}

public class ChainExpression<TInitial, TIn, TOut>
{
    private Expression<Func<TInitial, TOut>> _expression;

    public ChainExpression(Expression<Func<TInitial, TOut>> expression)
    {
        _expression = expression;
    }

    public Expression<Func<TInitial, TOut>> GetExpression()
    {
        return _expression;
    }

    public ChainExpression<TInitial, TOut, TNewIn> AddChainWithSubstitution<TNewIn>(
        Expression<Func<TInitial, TOut, TNewIn>> newChainExp,
        Dictionary<string, Expression> args)
    {
        newChainExp = newChainExp.SubstituteArgs(args);
        return AddChain(newChainExp);
    }
    

    public ChainExpression<TInitial, TOut, TNewIn> AddChain<TNewIn>(
        Expression<Func<TInitial, TOut, TNewIn>> newChainExp)
    {
        var newMethodCall = (MethodCallExpression)newChainExp.Body;
        var newExpParentParam = newChainExp.Parameters.First();

        var searcher = new ConditionVisitor(e => e is MethodCallExpression m && !m.Arguments.Any(a => a is MethodCallExpression));
        var leaf = (MethodCallExpression)searcher.Find(newMethodCall).First();

        var newLeaf = leaf.Update(null,  new[] { _expression.Body, leaf.Arguments[1] });
        var replacer = new ReplacerVisitor(new Dictionary<Expression, Expression>
        {
            { leaf, newLeaf }
        },
            customReplace: node => node is ParameterExpression par && par.Name == newExpParentParam.Name
                ? _expression.Parameters.First()
                : null);

        var newExp = replacer.VisitAndConvert(newMethodCall, null);
        
        return new ChainExpression<TInitial, TOut, TNewIn>(Expression.Lambda<Func<TInitial, TNewIn>>(newExp, _expression.Parameters.First()));
    }
}
