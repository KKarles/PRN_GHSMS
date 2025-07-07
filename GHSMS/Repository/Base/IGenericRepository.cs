using System.Linq.Expressions;

namespace Repository.Base
{
    public interface IGenericRepository<T> where T : class
    {
        // Synchronous methods
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includes);
        T? GetById(object id);
        T? GetById(object id, params Expression<Func<T, object>>[] includes);
        T Create(T entity);
        T Update(T entity);
        bool Remove(T entity);
        bool Remove(object id);
        void Save();

        // Asynchronous methods
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync(object id);
        Task<T?> GetByIdAsync(object id, params Expression<Func<T, object>>[] includes);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> RemoveAsync(T entity);
        Task<bool> RemoveAsync(object id);
        Task SaveAsync();

        // Preparation methods (for transaction management)
        T PrepareCreate(T entity);
        T PrepareUpdate(T entity);
        bool PrepareRemove(T entity);
        bool PrepareRemove(object id);

        // Query methods
        IQueryable<T> GetQueryable();
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}