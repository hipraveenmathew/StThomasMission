using Microsoft.Extensions.Options;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Core.Models.Settings;
using StThomasMission.Services.Exceptions;
using StThomasMission.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IAuditService _auditService;

        public CommunicationService(
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _auditService = auditService;
        }

        public async Task SendAnnouncementToWardAsync(BroadcastRequest request, string userId)
        {
            var recipients = await _unitOfWork.Families.GetFamilyContactsByWardAsync(request.TargetId);

            foreach (var recipient in recipients)
            {
                // Simple template replacement
                string personalizedMessage = request.Message.Replace("{RecipientName}", recipient.FirstName);

                await SendMessageAsync(request.Channel, recipient, personalizedMessage, MessageType.Announcement, userId);
            }

            await _auditService.LogActionAsync(userId, "Broadcast", "Ward", request.TargetId.ToString(), $"Sent announcement to Ward ID {request.TargetId} via {request.Channel}.");
        }

        public async Task SendUpdateToGroupAsync(BroadcastRequest request, string userId)
        {
            var recipients = await _unitOfWork.Students.GetParentContactsByGroupIdAsync(request.TargetId);
            var group = await _unitOfWork.Groups.GetByIdAsync(request.TargetId);
            if (group == null) throw new NotFoundException("Group", request.TargetId);

            foreach (var recipient in recipients)
            {
                string personalizedMessage = request.Message
                    .Replace("{ParentName}", recipient.FirstName)
                    .Replace("{StudentName}", recipient.StudentName)
                    .Replace("{GroupName}", group.Name);

                await SendMessageAsync(request.Channel, recipient, personalizedMessage, MessageType.GroupUpdate, userId);
            }

            await _auditService.LogActionAsync(userId, "Broadcast", "Group", request.TargetId.ToString(), $"Sent update to Group ID {request.TargetId} via {request.Channel}.");
        }

        private async Task SendMessageAsync(CommunicationChannel channel, RecipientContactInfo recipient, string message, MessageType messageType, string userId)
        {
            string status = "Failed";
            string? responseDetails = "No contact info available.";

            try
            {
                switch (channel)
                {
                    case CommunicationChannel.Email:
                        if (!string.IsNullOrWhiteSpace(recipient.Email))
                        {
                            (status, responseDetails) = await _emailSender.SendEmailAsync(recipient.Email, messageType.ToString(), message);
                        }
                        break;
                    case CommunicationChannel.SMS:
                        if (!string.IsNullOrWhiteSpace(recipient.PhoneNumber))
                        {
                            (status, responseDetails) = await _smsSender.SendSmsAsync(recipient.PhoneNumber, message);
                        }
                        break;
                    case CommunicationChannel.WhatsApp:
                        if (!string.IsNullOrWhiteSpace(recipient.PhoneNumber))
                        {
                            (status, responseDetails) = await _smsSender.SendWhatsAppAsync(recipient.PhoneNumber, message);
                        }
                        break;
                }
            }
            finally
            {
                // Always log the attempt
                await _unitOfWork.MessageLogs.LogMessageAsync(
                    recipient.Email ?? recipient.PhoneNumber ?? "Unknown",
                    message,
                    channel,
                    messageType,
                    status,
                    responseDetails,
                    userId);
            }
        }
    }
}