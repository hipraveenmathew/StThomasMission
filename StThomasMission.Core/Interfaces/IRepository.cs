using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// A generic repository interface defining common data access operations for an entity T.
    /// </summary>
    /// <typeparam name="T">The entity type, which must be a class.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its integer primary key.
        /// </summary>
        /// <param name="id">The entity's primary key.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Lists all entities from the database in a read-only, non-tracking query.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        Task<IEnumerable<T>> ListAllAsync();

        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity, including any database-generated values.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Marks an existing entity as updated.
        /// The actual database save is handled by the Unit of Work.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by its integer primary key.
        /// The actual database save is handled by the Unit of Work.
        /// </summary>
        /// <param name="id">The primary key of the entity to delete.</param>
        Task DeleteAsync(int id);

        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
}