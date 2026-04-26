using System.Linq.Expressions;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly KhadamatDbContext _dbContext;

    public GenericRepository(KhadamatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync(int id, string includeProperties = "")
    {
        IQueryable<T> query = _dbContext.Set<T>();
        foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync(string includeProperties = "")
    {
        IQueryable<T> query = _dbContext.Set<T>();
        foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }
        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> filter, string includeProperties = "")
    {
        IQueryable<T> query = _dbContext.Set<T>();
        foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }
        return await query.Where(filter).ToListAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbContext.Set<T>();
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return await query.CountAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, string includeProperties = "")
    {
        IQueryable<T> query = _dbContext.Set<T>();
        foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<IReadOnlyList<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "")
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }
}
