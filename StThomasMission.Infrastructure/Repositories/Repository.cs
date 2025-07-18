using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    /// <summary>
    /// A generic repository providing base CRUD functionality for any entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly StThomasMissionDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(StThomasMissionDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> ListAllAsync()
        {
            // Use AsNoTracking for read-only queries to improve performance.
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            // The Unit of Work will be responsible for calling SaveChangesAsync.
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            // This method is synchronous because it only changes the entity's state in the DbContext.
            // The actual I/O operation happens when SaveChangesAsync is called by the Unit of Work.
            // Using Update is versatile as it works for both tracked and detached entities.
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                // The Unit of Work will call SaveChangesAsync.
            }
        }
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}