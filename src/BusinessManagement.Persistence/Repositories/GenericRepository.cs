using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagement.Persistence;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly BusinessManagementDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(BusinessManagementDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool ignoreQueryFilters = false)
    {
        var query = _dbSet.AsNoTracking();
        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, bool ignoreQueryFilters = false)
    {
        var query = _dbSet.AsQueryable();
        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }
        await query.Where(predicate).ExecuteDeleteAsync();
    }
}





