using System.Data.Entity;
using System.Linq.Expressions;
using Chatiks.Tools.Extensions;
using Chatiks.Tools.Extensions.Models;
using LinqSpecs;
using Mapster;

namespace Chatiks.Chat.Specifications;

public abstract class SpecificationBase<TEntity> where TEntity: class
{
    private Specification<TEntity>? _filter;
    private int _limit = -1;
    private int _offset = -1;

    protected SpecificationBase(Specification<TEntity> filter = null)
    {
        _filter = filter;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        query = Include(_filter == null ? query : query.Where(_filter));

        if (_offset > 0)
        {
            query = query.Skip(_offset);
        }
        
        if (_limit > 0)
        {
            query = query.Take(_limit);
        }

        return query;
    }

    public void SetOffset(int offset)
    {
        _offset = offset;
    }

    public void SetLimit(int limit)
    {
        _limit = limit;
    }

    protected abstract IQueryable<TEntity> Include(IQueryable<TEntity> query);

    protected Expression<Func<TEntity, IEnumerable<TChild>>> CreateIncludeAndOrderExpressionFromFilter<TChild, TOrderKey>(
        Expression<Func<TEntity, IEnumerable<TChild>>> childSelector,
        Expression<Func<TChild, bool>> filter,
        Expression<Func<TChild, TOrderKey>> orderExp,
        bool desc = false,
        int limit = -1)
    {
        if (limit < 0)
        {
            limit = int.MaxValue;
        }
        
        var exp = CreateIncludeExpressionFromFilter(childSelector, filter);

        Expression<Func<TEntity, IEnumerable<TChild>>> includeOrderExp;
        if (!desc)
        {
            includeOrderExp = ChainExpressionFactory.FromLambda(exp)
                .AddChainWithSubstitution((p, c) => c
                        .OrderBy(new SubstituteArg<Func<TChild, TOrderKey>>("order").GetValue())
                        .Take(limit),
                    new Dictionary<string, Expression>
                    {
                        { "order", orderExp },
                    })
                .GetExpression();
        }
        else
        {
            includeOrderExp = ChainExpressionFactory.FromLambda(exp)
                .AddChainWithSubstitution((p, c) => c
                        .OrderByDescending(new SubstituteArg<Func<TChild, TOrderKey>>("order").GetValue())
                        .Take(limit),
                    new Dictionary<string, Expression>
                    {
                        { "order", orderExp },
                    })
                .GetExpression();
        }

        return includeOrderExp;
    }
    
    protected Expression<Func<TEntity, IEnumerable<TChild>>> CreateIncludeExpressionFromFilter<TChild>(
        Expression<Func<TEntity, IEnumerable<TChild>>> childSelector,
        Expression<Func<TChild, bool>> filter
       )
    {
        return ChainExpressionFactory.FromLambda(childSelector)
            .AddChainWithSubstitution((p, c) => c
                .Where(new SubstituteArg<Func<TChild, bool>>("filter").GetValue()), new Dictionary<string, Expression>
            {
                { "filter", filter },
            })
            .GetExpression();
    }
}