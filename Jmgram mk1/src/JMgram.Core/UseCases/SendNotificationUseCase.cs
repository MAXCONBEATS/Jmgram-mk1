

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class SendNotificationUseCase
    {
        private readonly INotificationRepository _notificationRepository;

        public SendNotificationUseCase(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }

        public async Task<SendNotificationResponse> Execute(NotificationDto notificationDto)
        {
            try
            {
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

                return new SendNotificationResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new SendNotificationResponse { IsSuccess = false, ErrorMessage = $"Ошибка при отправке уведомления: {ex.Message}" };
            }
        }
    }

}
