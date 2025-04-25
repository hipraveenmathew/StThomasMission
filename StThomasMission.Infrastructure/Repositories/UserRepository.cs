using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for managing User entities.
    /// </summary>
    public class UserRepository : IRepository<User>
    {
        private readonly DbContext _context;
        private readonly DbSet<User> _users;

        public UserRepository(DbContext context)
        {
            _context = context;
            _users = context.Set<User>();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _users.ToListAsync();
        }

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
        {
            return await _users.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(User entity)
        {
            await _users.AddAsync(entity);
        }

        public async Task UpdateAsync(User entity)
        {
            _users.Update(entity);
        }

        public async Task DeleteAsync(User entity)
        {
            _users.Remove(entity);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
        {
            return await _users.Where(u => u.Role == role).ToListAsync();
        }

        public Task<IEnumerable<User>> GetAsync(Expression<Func<User, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}