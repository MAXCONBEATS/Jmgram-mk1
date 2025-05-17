

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

        public async Task<SendNotificationResponse> Execute(NotificationDto notificationDto, string senderUserId) // Add senderUserId
        {
            try
            {
                _logger.LogInformation($"SendNotificationUseCase.Execute: Attempting to send notification of type {notificationDto.NotificationType} from UserId: {senderUserId}");

                // Check if the user is trying to send a system notification
                if (notificationDto.NotificationType == NotificationType.System) // Assuming System = 2 is the system notification type
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
                            userId = notificationDto.UserId,
                            message = notificationDto.Message,
                            timestamp = notificationDto.Timestamp,
                            isRead = notificationDto.IsRead,
                        };
                        break;
                    case NotificationType.ContactRequest:
                        notification = new ContactRequestNotification
                        {
                            userId = notificationDto.UserId,
                            message = notificationDto.Message,
                            timestamp = notificationDto.Timestamp,
                            isRead = notificationDto.IsRead,
                        };
                        break;
                    case NotificationType.System:
                        notification = new SystemNotification
                        {
                            userId = notificationDto.UserId,
                            message = notificationDto.Message,
                            timestamp = notificationDto.Timestamp,
                            isRead = notificationDto.IsRead,
                        };
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
                _logger.LogError($"SendNotificationUseCase.Execute: An error occurred while sending notification: {ex.Message}");
                return new SendNotificationResponse { IsSuccess = false, ErrorMessage = $"Ошибка при отправке уведомления: {ex.Message}" };
            }
        }
    }

}
