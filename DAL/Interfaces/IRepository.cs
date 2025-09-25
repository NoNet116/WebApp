using System.Linq.Expressions;

namespace DAL.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

    Task<T?> GetByIdAsync(object id);

    Task AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T Item);

    IQueryable<T> GetQueryable();

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
}