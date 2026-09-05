using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BusinessManagement.Domain.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool ignoreQueryFilters = false);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, bool ignoreQueryFilters = false);
}


