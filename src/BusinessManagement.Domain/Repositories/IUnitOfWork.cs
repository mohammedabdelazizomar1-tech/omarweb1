using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessManagement.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> GetRepository<T>() where T : class;
    Task<int> CompleteAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


