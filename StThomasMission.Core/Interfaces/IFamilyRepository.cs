using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyRepository : IRepository<Family>
    {
        Task<FamilyDetailDto?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber);

        Task<IEnumerable<FamilySummaryDto>> GetByWardAsync(int wardId);
        Task<IEnumerable<RecipientContactInfo>> GetFamilyContactsByWardAsync(int wardId);
        Task<FamilyDetailDto?> GetFamilyDetailByIdAsync(int familyId);
        Task<IEnumerable<FamilyRegistryDto>> GetAllFamilyRegistrationsAsync();


        Task<IPaginatedList<FamilySummaryDto>> SearchFamiliesPaginatedAsync(int pageNumber, int pageSize, string? searchTerm = null, int? wardId = null, bool? isRegistered = null);
    }
}