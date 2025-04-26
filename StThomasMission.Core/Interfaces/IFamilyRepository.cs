using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyRepository : IRepository<Family>
    {
        Task<Family?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber);
        Task<Family?> GetByTemporaryIdAsync(string temporaryId);
        Task<IEnumerable<Family>> GetByWardAsync(int wardId);
        Task<IEnumerable<Family>> GetByStatusAsync(FamilyStatus status);
        IQueryable<Family> GetQueryable(Expression<Func<Family, bool>> predicate);
        IQueryable<Family> GetAllQueryable();
    }
}