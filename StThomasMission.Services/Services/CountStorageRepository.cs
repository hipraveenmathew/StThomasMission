using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Linq.Expressions;

namespace StThomasMission.Services.Services
{
    public class CountStorageRepository : ICountStorageRepository
    {
        private readonly StThomasMissionDbContext _context;

        public CountStorageRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CountStorage entity)
        {
            _context.CountStorages.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.CountStorages.FindAsync(id);
            if (entity != null)
            {
                _context.CountStorages.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CountStorage>> GetAllAsync()
        {
            return await _context.CountStorages.ToListAsync();
        }

        public async Task<IEnumerable<CountStorage>> GetAsync(Expression<Func<CountStorage, bool>> predicate)
        {
            return await _context.CountStorages.Where(predicate).ToListAsync();
        }

        public async Task<CountStorage?> GetByIdAsync(int id)
        {
            return await _context.CountStorages.FindAsync(id);
        }

        public async Task UpdateAsync(CountStorage entity)
        {
            _context.CountStorages.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
