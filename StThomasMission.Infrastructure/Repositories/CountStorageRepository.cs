using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Data;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories

{
    public class CountStorageRepository : Repository<CountStorage>, ICountStorageRepository
    {
        public CountStorageRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<int> GetNextValueAsync(string counterName)
        {
            // This operation must be atomic to prevent race conditions.
            // We use an explicit transaction and SQL row locking to ensure that
            // two concurrent requests will not receive the same number.
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                // Find the specific counter row and lock it for update.
                var counter = await _dbSet.FromSqlRaw(
                        "SELECT * FROM CountStorages WITH (UPDLOCK, ROWLOCK) WHERE CounterName = {0}",
                        counterName)
                    .FirstOrDefaultAsync();

                if (counter == null)
                {
                    throw new InvalidOperationException($"Counter '{counterName}' not found. Please seed the database.");
                }

                // Increment the value
                counter.LastValue++;

                // Save the change immediately within this transaction
                await _context.SaveChangesAsync();

                // Commit the transaction to release the lock
                await transaction.CommitAsync();

                return counter.LastValue;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception after rolling back
            }
        }
    }
}