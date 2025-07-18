using System.Threading.Tasks;

namespace StThomasMission.Services.Interfaces
{
    public interface ISmsSender
    {
        Task<(string Status, string? Details)> SendSmsAsync(string toNumber, string message);
        Task<(string Status, string? Details)> SendWhatsAppAsync(string toNumber, string message);
    }
}