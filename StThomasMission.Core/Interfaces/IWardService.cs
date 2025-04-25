using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing Ward operations.
    /// </summary>
    public interface IWardService
    {
        Task<Ward> CreateWardAsync(string name);
        Task UpdateWardAsync(int wardId, string name);
        Task DeleteWardAsync(int wardId);
        Task<Ward?> GetWardByIdAsync(int wardId);
        Task<IEnumerable<Ward>> GetAllWardsAsync();
    }
}