using StThomasMission.Core.DTOs;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<DashboardViewModelDto> GetDashboardSummaryAsync(string userId);

    }
}