using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Linq.Expressions;

namespace Repository.Base
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly GenderHealthcareDBContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(GenderHealthcareDBContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Synchronous methods
        public virtual IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.ToList();
        }

        public virtual T? GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public virtual T? GetById(object id, params Expression<Func<T, object>>[] includes)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
            {
                foreach (var include in includes)
                {
                    _context.Entry(entity).Reference(include).Load();
                }
            }
            return entity;
        }

        public virtual T Create(T entity)
        {
            var result = _dbSet.Add(entity);
            _context.SaveChanges();
            return result.Entity;
        }

        public virtual T Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
            return entity;
        }

        public virtual bool Remove(T entity)
        {
            _dbSet.Remove(entity);
            return _context.SaveChanges() > 0;
        }

        public virtual bool Remove(object id)
        {
            var entity = GetById(id);
            if (entity == null) return false;
            return Remove(entity);
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }

        // Asynchronous methods
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetByIdAsync(object id, params Expression<Func<T, object>>[] includes)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                foreach (var include in includes)
                {
                    await _context.Entry(entity).Reference(include).LoadAsync();
                }
            }
            return entity;
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            var result = await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<bool> RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> RemoveAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            return await RemoveAsync(entity);
        }

        public virtual async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        // Preparation methods (for transaction management)
        public virtual T PrepareCreate(T entity)
        {
            var result = _dbSet.Add(entity);
            return result.Entity;
        }

        public virtual T PrepareUpdate(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public virtual bool PrepareRemove(T entity)
        {
            _dbSet.Remove(entity);
            return true;
        }

        public virtual bool PrepareRemove(object id)
        {
            var entity = GetById(id);
            if (entity == null) return false;
            return PrepareRemove(entity);
        }

        // Query methods
        public virtual IQueryable<T> GetQueryable()
        {
            return _dbSet;
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }
    }
}