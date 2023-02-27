using Microsoft.EntityFrameworkCore;

namespace Chatiks.Tools.EF;

public abstract class RepositoryBase<TContext> where TContext : DbContext
{
    protected readonly TContext Context;

    public RepositoryBase(TContext context)
    {
        Context = context;
    }
    
    protected IQueryable<TEntity> GetAllDetached<TEntity>() where TEntity: class
    {
        return GetAll<TEntity>().AsNoTracking();
    }
    
    protected IQueryable<TEntity> GetAll<TEntity>() where TEntity: class
    {
        return Context.Set<TEntity>();
    }

    public Task<int> SaveChangesAsync()
    {
        return Context.SaveChangesAsync();
    }

    public IsolatedOperationScope<TContext> BeginIsolatedOperation(bool saveChanges = false)
    {
        return new IsolatedOperationScope<TContext>(Context, saveChanges);
    }
}