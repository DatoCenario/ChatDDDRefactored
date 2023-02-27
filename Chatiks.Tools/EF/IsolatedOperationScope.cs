using Microsoft.EntityFrameworkCore;

namespace Chatiks.Tools.EF;

public class IsolatedOperationScope<TContext> : IDisposable where TContext: DbContext
{
    private readonly TContext _context;
    private bool _saveChanges;

    public IsolatedOperationScope(TContext context, bool saveChanges = false)
    {
        _context = context;
        _saveChanges = saveChanges;
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        if (_saveChanges)
            _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }
}