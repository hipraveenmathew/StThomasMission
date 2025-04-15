using StThomasMission.Core.Entities;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}
