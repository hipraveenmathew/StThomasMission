using StThomasMission.Core.Entities;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface ICountStorageRepository : IRepository<CountStorage>
    {
        /// <summary>
        /// Atomically retrieves the next value for a given counter and increments it in the database.
        /// </summary>
        /// <param name="counterName">The name of the counter (e.g., "ChurchRegistrationNumber").</param>
        /// <returns>The next integer value in the sequence.</returns>
        Task<int> GetNextValueAsync(string counterName);
    }
}