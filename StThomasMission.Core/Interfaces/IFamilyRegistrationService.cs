using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyRegistrationService
    {
        Task<Family> CreateNewFamilyFromImportAsync(ImportFamilyData data, string userId);
    }
}