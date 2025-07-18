using Microsoft.Extensions.Caching.Memory;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private const string DashboardCacheKey = "DashboardSummary";

        public DashboardService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            // Try to get the summary from the cache first.
            if (_cache.TryGetValue(DashboardCacheKey, out DashboardSummaryDto? cachedSummary))
            {
                if (cachedSummary != null) return cachedSummary;
            }

            // If not in cache, query the database efficiently.
            var summary = await CreateSummaryFromDb();

            // Store the result in the cache with a 5-minute expiration.
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(DashboardCacheKey, summary, cacheEntryOptions);

            return summary;
        }
        public async Task<DashboardViewModelDto> GetDashboardSummaryAsync(string userId)
        {
            // Fetch all data in parallel
            var totalStudentsTask = _unitOfWork.Students.CountAsync();
            var activeStudentsTask = _unitOfWork.Students.CountAsync(s => s.Status == Core.Enums.StudentStatus.Active);
            var graduatedStudentsTask = _unitOfWork.Students.CountAsync(s => s.Status == Core.Enums.StudentStatus.Graduated);
            var totalFamiliesTask = _unitOfWork.Families.CountAsync();
            var registeredFamiliesTask = _unitOfWork.Families.CountAsync(f => f.IsRegistered);
            var totalWardsTask = _unitOfWork.Wards.CountAsync();
            var totalGroupsTask = _unitOfWork.Groups.CountAsync();
            var announcementsTask = _unitOfWork.Announcements.GetActiveAnnouncementsAsync();
            var eventsTask = _unitOfWork.GroupActivities.GetUpcomingActivitiesAsync(DateTime.UtcNow, 5);

            await Task.WhenAll(
                totalStudentsTask, activeStudentsTask, graduatedStudentsTask, totalFamiliesTask,
                registeredFamiliesTask, totalWardsTask, totalGroupsTask, announcementsTask, eventsTask
            );

            return new DashboardViewModelDto
            {
                TotalStudents = await totalStudentsTask,
                ActiveStudents = await activeStudentsTask,
                GraduatedStudents = await graduatedStudentsTask,
                TotalFamilies = await totalFamiliesTask,
                RegisteredFamilies = await registeredFamiliesTask,
                TotalWards = await totalWardsTask,
                TotalGroups = await totalGroupsTask,
                RecentAnnouncements = (await announcementsTask).OrderByDescending(a => a.PostedDate).Take(5).ToList(),
                UpcomingEvents = await eventsTask
            };
        }

        private async Task<DashboardSummaryDto> CreateSummaryFromDb()
        {
            // Perform all count operations asynchronously and in parallel for maximum efficiency.
            var totalStudentsTask = _unitOfWork.Students.CountAsync();
            var activeStudentsTask = _unitOfWork.Students.CountAsync(s => s.Status == StudentStatus.Active);
            var graduatedStudentsTask = _unitOfWork.Students.CountAsync(s => s.Status == StudentStatus.Graduated);
            var migratedStudentsTask = _unitOfWork.Students.CountAsync(s => s.Status == StudentStatus.Migrated);

            var totalFamiliesTask = _unitOfWork.Families.CountAsync();
            var registeredFamiliesTask = _unitOfWork.Families.CountAsync(f => f.IsRegistered);

            var totalWardsTask = _unitOfWork.Wards.CountAsync();
            var totalGroupsTask = _unitOfWork.Groups.CountAsync();

            await Task.WhenAll(
                totalStudentsTask, activeStudentsTask, graduatedStudentsTask, migratedStudentsTask,
                totalFamiliesTask, registeredFamiliesTask, totalWardsTask, totalGroupsTask);

            var summary = new DashboardSummaryDto
            {
                TotalStudents = await totalStudentsTask,
                ActiveStudents = await activeStudentsTask,
                GraduatedStudents = await graduatedStudentsTask,
                MigratedStudents = await migratedStudentsTask,
                TotalFamilies = await totalFamiliesTask,
                RegisteredFamilies = await registeredFamiliesTask,
                UnregisteredFamilies = (await totalFamiliesTask) - (await registeredFamiliesTask),
                TotalWards = await totalWardsTask,
                TotalGroups = await totalGroupsTask,
                GeneratedAt = DateTime.UtcNow
            };
            return summary;
        }
    }
}