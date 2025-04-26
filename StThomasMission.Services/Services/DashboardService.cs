using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            var families = await _unitOfWork.Families.GetAllAsync();
            var wards = await _unitOfWork.Wards.GetAllAsync();
            var groups = await _unitOfWork.Groups.GetAllAsync();
            var activities = await _unitOfWork.GroupActivities.GetAllAsync();

            return new DashboardSummaryDto
            {
                TotalStudents = students.Count(),
                ActiveStudents = students.Count(s => s.Status == StudentStatus.Active),
                GraduatedStudents = students.Count(s => s.Status == StudentStatus.Graduated),
                MigratedStudents = students.Count(s => s.Status == StudentStatus.Migrated),
                InactiveStudents = students.Count(s => s.Status == StudentStatus.Inactive),
                DeletedStudents = students.Count(s => s.Status == StudentStatus.Deleted),
                TotalFamilies = families.Count(),
                RegisteredFamilies = families.Count(f => f.IsRegistered),
                UnregisteredFamilies = families.Count(f => !f.IsRegistered),
                TotalWards = wards.Count(),
                TotalGroups = groups.Count(),
                TotalActivities = activities.Count()
            };
        }
    }
}