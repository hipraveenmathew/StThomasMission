using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyService _familyService;

        public DashboardService(IUnitOfWork unitOfWork, IFamilyService familyService)
        {
            _unitOfWork = unitOfWork;
            _familyService = familyService;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            var families = await _familyService.GetFamiliesByStatusAsync(FamilyStatus.Active);
            var groupActivities = await _unitOfWork.GroupActivities.GetAllAsync();

            return new DashboardSummaryDto
            {
                TotalStudents = students.Count(),
                ActiveStudents = students.Count(s => s.Status == StudentStatus.Active),
                GraduatedStudents = students.Count(s => s.Status == StudentStatus.Graduated),
                MigratedStudents = students.Count(s => s.Status == StudentStatus.Migrated),

                TotalFamilies = families.Count(),
                RegisteredFamilies = families.Count(f => f.IsRegistered),
                UnregisteredFamilies = families.Count(f => !f.IsRegistered),

                TotalGroups = students.Where(s => !string.IsNullOrEmpty(s.Group)).Select(s => s.Group).Distinct().Count(),
                TotalActivities = groupActivities.Count()
            };
        }
    }
}