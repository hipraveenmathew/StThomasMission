using StThomasMission.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface ICommunicationService
    {
        Task SendAnnouncementAsync(string message, int wardId, string method);
        Task SendAbsenteeNotificationsAsync(string grade, string method);
        Task SendFeeReminderAsync(int familyId, string feeDetails);
        Task SendGroupUpdateAsync(int groupId, string updateMessage, string method);
        IQueryable<MessageLog> GetMessageHistoryQueryable(string searchString = null);
    }
}