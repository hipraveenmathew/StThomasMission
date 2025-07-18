using StThomasMission.Core.DTOs;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyService
    {
        Task<FamilyDetailDto> RegisterFamilyAsync(RegisterFamilyRequest request, string userId);

        Task UpdateFamilyDetailsAsync(int familyId, UpdateFamilyDetailsRequest request, string userId);

        Task ConvertToRegisteredFamilyAsync(int familyId, string userId);

        Task MigrateFamilyAsync(int familyId, MigrateFamilyRequest request, string userId);
        Task DeleteFamilyAsync(int familyId, string userId);
        Task<IPaginatedList<FamilySummaryDto>> SearchFamiliesPaginatedAsync(int pageNumber, int pageSize, string? searchTerm = null, int? wardId = null, bool? isRegistered = null);
        Task<FamilyDetailDto> GetFamilyDetailByIdAsync(int familyId);

    }
}