

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class SendNotificationUseCase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<SendNotificationUseCase> _logger;

        public SendNotificationUseCase(INotificationRepository notificationRepository, ILogger<SendNotificationUseCase> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendNotificationResponse> Execute(NotificationDto notificationDto, string senderUserId)
        {
            try
            {
                _logger.LogInformation($"SendNotificationUseCase.Execute: Attempting to send notification of type {notificationDto.NotificationType} from UserId: {senderUserId}");

                if (notificationDto.NotificationType == NotificationType.System)
                {
                    _logger.LogWarning($"SendNotificationUseCase.Execute: User {senderUserId} attempted to send a system notification, which is not allowed.");
                    return new SendNotificationResponse { IsSuccess = false, ErrorMessage = "Отправка системных уведомлений от пользователей запрещена." };
                }

                Notification notification;

                switch (notificationDto.NotificationType)
                {
                    case NotificationType.Message:
                        notification = new MessageNotification
                        {
                            UserId = notificationDto.UserId,
                            Message = notificationDto.Message,
                            Timestamp = notificationDto.Timestamp,
                            IsRead = notificationDto.IsRead,
                            ChatId = notificationDto.ChatId
                        };
                        break;
                    case NotificationType.ContactRequest:
                        notification = new ContactRequestNotification
                        {
                            UserId = notificationDto.UserId,
                            Message = notificationDto.Message,
                            Timestamp = notificationDto.Timestamp,
                            IsRead = notificationDto.IsRead,
                            ChatId = notificationDto.ChatId
                        };
                        break;
                    case NotificationType.System:
                        notification = new SystemNotification
                        {
                            UserId = notificationDto.UserId,
                            Message = notificationDto.Message,
                            Timestamp = notificationDto.Timestamp,
                            IsRead = notificationDto.IsRead,
                            ChatId = notificationDto.ChatId
                        };
                        break;
                    case NotificationType.ChatInvite:
                        notification = new ChatInviteNotification
                        {
                            UserId = notificationDto.UserId,
                            Message = notificationDto.Message,
                            Timestamp = notificationDto.Timestamp,
                            IsRead = notificationDto.IsRead,
                            ChatId = notificationDto.ChatId
                        };
                        if (notificationDto != null)
                        {
                            _logger.LogInformation($"notificationDto не null");
                        }
                        else
                        {
                            _logger.LogInformation($"notificationDto  null");
                        }
                        if (notificationDto.ChatId != null)
                        {
                            _logger.LogInformation($"ChatId не null");
                        }
                        else
                        {
                            _logger.LogInformation($"ChatId null");
                        }
                        break;
                    default:
                        return new SendNotificationResponse { IsSuccess = false, ErrorMessage = $"Unsupported notification type: {notificationDto.NotificationType}" };
                }

                await _notificationRepository.AddNotification(notification);

                _logger.LogInformation($"SendNotificationUseCase.Execute: Notification sent successfully.");
                return new SendNotificationResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendNotificationUseCase.Execute: An error occurred while sending notification: {ex.Message}, Inner Exception: {ex.InnerException}");
                return new SendNotificationResponse { IsSuccess = false, ErrorMessage = $"Ошибка при отправке уведомления: {ex.Message}" };
            }
        }
    }

}
