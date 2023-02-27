using System.Linq.Expressions;
using Chatiks.Tools.Extensions.Models;

namespace Chatiks.Tools.Extensions;

public static class ExpressionsExtensions
{
    public static Expression<TFunc> SubstituteArgs<TFunc>(this Expression<TFunc> exp, Dictionary<string, Expression> substives)
    {
        var Substituter = new SubstituteVisitor(substives);
        return Substituter.VisitAndConvert(exp, null);
    }
}