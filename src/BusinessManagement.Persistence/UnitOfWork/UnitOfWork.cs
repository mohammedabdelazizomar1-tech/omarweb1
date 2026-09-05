using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly BusinessManagementDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private bool _disposed = false;

    public UnitOfWork(BusinessManagementDbContext context)
    {
        _context = context;
    }

    public IRepository<T> GetRepository<T>() where T : class
    {
        return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_context));
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}





