using StThomasMission.Core.DTOs;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface ICommunicationService
    {
        Task SendAnnouncementToWardAsync(BroadcastRequest request, string userId);
        Task SendUpdateToGroupAsync(BroadcastRequest request, string userId);
    }
}