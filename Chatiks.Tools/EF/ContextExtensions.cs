using Chatiks.Chat.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Tools.EF;

public static class ContextExtensions
{
    public static IsolatedOperationScope<TContext> BeginIsolatedOperation<TContext>(this TContext context, bool saveChanges = false) where TContext: DbContext
    {
        return new IsolatedOperationScope<TContext>(context, saveChanges);
    }
    
    public static async Task<ICollection<TEntity>> LoadBySpecificationAsync<TEntity>(this DbSet<TEntity> set, SpecificationBase<TEntity> specification) where TEntity: class
    {
        var query = set.AsNoTracking();

        if (specification != null)
        {
            query = specification.Apply(query);
        }

        return await query.ToArrayAsync();
    }
    
    public static async Task<TEntity> FirstOrDefaultBySpecificationAsync<TEntity>(this DbSet<TEntity> set, SpecificationBase<TEntity> specification) where TEntity: class
    {
        var query = set.AsNoTracking();

        if (specification != null)
        {
            query = specification.Apply(query);
        }

        return await query.FirstOrDefaultAsync();
    }
    
    public static async Task<ICollection<TEntity>> LoadBySpecificationAsync<TEntity, TContext>(this TContext context, SpecificationBase<TEntity> specification) where TEntity: class where TContext: DbContext
    {
        var query = context.Set<TEntity>().AsNoTracking();

        if (specification != null)
        {
            query = specification.Apply(query);
        }

        return await query.ToArrayAsync();
    }
}