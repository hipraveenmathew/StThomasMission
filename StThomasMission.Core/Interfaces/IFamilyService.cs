using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyService
    {
        Task<Family> RegisterFamilyAsync(string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId);
        Task UpdateFamilyAsync(int familyId, string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, FamilyStatus status, string? migratedTo);
        Task ConvertTemporaryIdToChurchIdAsync(int familyId, string churchRegistrationNumber);
        Task DeleteFamilyAsync(int familyId);
        Task<Family?> GetFamilyByIdAsync(int familyId);
        Task<IEnumerable<Family>> GetFamiliesByWardAsync(int wardId);
        Task<IEnumerable<Family>> GetFamiliesByStatusAsync(FamilyStatus status);
        Task<byte[]> GenerateRegistrationSlipAsync(int familyId);
    }
}