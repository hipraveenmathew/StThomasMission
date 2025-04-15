using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string action, string entityName, int entityId, string details);
    }
}