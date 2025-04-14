using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyMemberRepository : IRepository<FamilyMember>
    {
        Task<FamilyMember> GetByIdAsync(int id);
    }
}