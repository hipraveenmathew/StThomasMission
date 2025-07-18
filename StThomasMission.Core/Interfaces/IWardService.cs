using StThomasMission.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IWardService
    {
        Task<WardDetailDto> GetWardByIdAsync(int wardId);

        Task<IEnumerable<WardDetailDto>> GetAllWardsAsync();

        Task<WardDetailDto> CreateWardAsync(CreateWardRequest request, string userId);

        Task UpdateWardAsync(int wardId, UpdateWardRequest request, string userId);

        Task DeleteWardAsync(int wardId, string userId);
    }
}