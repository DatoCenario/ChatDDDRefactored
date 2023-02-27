using Chatiks.Chat.Specifications;

namespace Chatiks.Tools.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> WithSpecification<TEntity>(this IQueryable<TEntity> query, SpecificationBase<TEntity> specification) 
        where TEntity: class
    {
        return specification.Apply(query);
    }
}