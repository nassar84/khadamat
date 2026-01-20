using System.Linq.Expressions;
using Khadamat.Domain.Entities;

namespace Khadamat.Application.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> filter);
    Task<IReadOnlyList<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "");
}
