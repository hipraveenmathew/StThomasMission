using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing GroupActivity operations.
    /// </summary>
    public interface IGroupActivityService
    {
        Task AddGroupActivityAsync(string name, string description, DateTime date, string group, int points);
        Task UpdateGroupActivityAsync(int groupActivityId, string name, string description, DateTime date, string group, int points, ActivityStatus status);
        Task DeleteGroupActivityAsync(int groupActivityId);
        Task AddStudentToGroupActivityAsync(int studentId, int groupActivityId, DateTime participationDate);
        Task<IEnumerable<GroupActivity>> GetGroupActivitiesAsync(string? group = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<StudentGroupActivity>> GetStudentGroupActivitiesAsync(int studentId);
        Task<List<GroupActivity>> GetAllGroupActivitiesAsync();
        Task<GroupActivity?> GetGroupActivityByIdAsync(int groupActivityId);
        Task<List<StudentGroupActivity>> GetStudentParticipationAsync(int groupActivityId);
        Task RecordStudentGroupActivityAsync(int studentId, int groupActivityId);
    }
}