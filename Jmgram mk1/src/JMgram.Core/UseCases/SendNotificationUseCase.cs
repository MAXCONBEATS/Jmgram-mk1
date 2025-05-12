

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

        public async Task<SendNotificationResponse> Execute(SendNotificationRequest request)
        {
            // 1. Валидация входных данных
            if (request == null)
            {
                return new SendNotificationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null.",
                    Notification = null
                };
            }

            if (request.Notification == null)
            {
                return new SendNotificationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Notification in request cannot be null.",
                    Notification = null
                };
            }

            if (request.Notification.UserId <= 0)
            {
                return new SendNotificationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "UserId must be greater than 0.",
                    Notification = null
                };
            }

            try
            {
                // 2. Преобразование NotificationDto в Notification Entity
                Notification notificationEntity = CreateNotificationEntity(request.Notification);

                if (notificationEntity == null)
                {
                    return new SendNotificationResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Unsupported notification type: {request.Notification.NotificationType}",
                        Notification = null
                    };
                }

                // 3. Отправка уведомления через репозиторий
                await _notificationRepository.AddNotification(notificationEntity);

                // 4. Формирование успешного ответа
                return new SendNotificationResponse
                {
                    IsSuccess = true,
                    Notification = request.Notification
                };
            }
            catch (Exception ex)
            {
                // 5. Обработка ошибок
                return new SendNotificationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while sending notification: {ex.Message}",
                    Notification = null
                };
            }
        }
        private Notification CreateNotificationEntity(NotificationDto notificationDto)
        {
            switch (notificationDto.NotificationType)
            {
                case NotificationType.Message:
                    if (notificationDto is MessageNotificationDto messageNotificationDto)
                    {
                        return new MessageNotification
                        {
                            userId = notificationDto.UserId,
                            message = notificationDto.Message,
                            timestamp = DateTime.UtcNow,
                            isRead = false,
                            NotificationMessageId = messageNotificationDto.MessageId
                        };
                    }
                    break;
                case NotificationType.ContactRequest:
                    if (notificationDto is ContactRequestNotificationDto contactRequestNotificationDto)
                    {
                        return new ContactRequestNotification
                        {
                            userId = notificationDto.UserId,
                            message = notificationDto.Message,
                            timestamp = DateTime.UtcNow,
                            isRead = false,
                            contactRequestId = contactRequestNotificationDto.ContactRequestId,
                            senderUserId = contactRequestNotificationDto.SenderUserId
                        };
                    }
                    break;
                case NotificationType.System:
                    if (notificationDto is SystemNotificationDto systemNotificationDto)
                    {
                        return new SystemNotification
                        {
                            userId = notificationDto.UserId,
                            message = notificationDto.Message,
                            timestamp = DateTime.UtcNow,
                            isRead = false,
                            source = systemNotificationDto.Source
                        };
                    }
                    break;
            }

            return null;
        }
    }

}
