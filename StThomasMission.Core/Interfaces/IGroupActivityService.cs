using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupActivityService
    {
        Task AddGroupActivityAsync(string name, string description, DateTime date, int groupId, int points);
        Task UpdateGroupActivityAsync(int groupActivityId, string name, string description, DateTime date, int groupId, int points, ActivityStatus status);
        Task DeleteGroupActivityAsync(int groupActivityId);
        Task AddStudentToGroupActivityAsync(int studentId, int groupActivityId, DateTime participationDate, int pointsEarned);
        Task<IEnumerable<GroupActivity>> GetGroupActivitiesAsync(int? groupId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<StudentGroupActivity>> GetStudentGroupActivitiesAsync(int studentId);
        Task<GroupActivity?> GetGroupActivityByIdAsync(int groupActivityId);
    }
}