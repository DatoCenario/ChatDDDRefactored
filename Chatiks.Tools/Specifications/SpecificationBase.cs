using System.Linq.Expressions;
using Chatiks.Tools.Extensions;
using Chatiks.Tools.Extensions.Models;
using LinqSpecs;

namespace Chatiks.Chat.Specifications;

public abstract class SpecificationBase<TEntity> where TEntity: class
{
    private Specification<TEntity>? _filter;

    protected SpecificationBase(Specification<TEntity> filter = null)
    {
        _filter = filter;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        return Include(_filter == null ? query : query.Where(_filter));
    }

    protected abstract IQueryable<TEntity> Include(IQueryable<TEntity> query);

    protected Expression<Func<TEntity, IEnumerable<TChild>>> CreateIncludeExpressionFromFilter<TChild>(
        Expression<Func<TEntity, IEnumerable<TChild>>> childSelector,
        Specification<TChild> filter)
    {
        var exp = filter.ToExpression();
        Expression<Func<TEntity, IEnumerable<TChild>>> includeExp = p =>
            new SubstituteArg<Func<TEntity, IEnumerable<TChild>>>("selector").GetValue()(p)
                .Where(new SubstituteArg<Func<TChild, bool>>("filter").GetValue());
        includeExp = includeExp.SubstituteArgs(new Dictionary<string, Expression>
        {
            { "selector", childSelector },
            { "filter", exp },
        });

        return includeExp;
    }
}