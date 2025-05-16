

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class SendNotificationUseCase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SendNotificationUseCase(INotificationRepository notificationRepository, IHttpContextAccessor httpContextAccessor)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<SendNotificationResponse> Execute(NotificationDto request)
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
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
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
                Notification notificationEntity = CreateNotificationEntity(request, userId);

                if (notificationEntity == null)
                {
                    return new SendNotificationResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Unsupported notification type: {request.NotificationType}",
                        Notification = null
                    };
                }

                // 3. Отправка уведомления через репозиторий
                await _notificationRepository.AddNotification(notificationEntity);

                // 4. Формирование успешного ответа
                return new SendNotificationResponse
                {
                    IsSuccess = true,
                    Notification = request
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
        private Notification CreateNotificationEntity(NotificationDto notificationDto, string userId)
        {
            switch (notificationDto.NotificationType)
            {
                case NotificationType.Message:
                    if (notificationDto is MessageNotificationDto messageNotificationDto)
                    {
                        return new MessageNotification
                        {
                            userId = userId,
                            message = notificationDto.Message,
                            timestamp = DateTime.UtcNow,
                            isRead = false,
                            MessageId = messageNotificationDto.MessageId
                        };
                    }
                    break;
                case NotificationType.ContactRequest:
                    if (notificationDto is ContactRequestNotificationDto contactRequestNotificationDto)
                    {
                        return new ContactRequestNotification
                        {
                            userId = userId,
                            message = notificationDto.Message,
                            timestamp = DateTime.UtcNow,
                            isRead = false,
                            senderUserId = contactRequestNotificationDto.SenderUserId
                        };
                    }
                    break;
                case NotificationType.System:
                    var isAdmin = true; //_httpContextAccessor.HttpContext.User.IsInRole("Admin");
                    if (!isAdmin)
                    {
                        return null;
                    }
                    if (notificationDto is SystemNotificationDto systemNotificationDto)
                    {
                        return new SystemNotification
                        {
                            userId = userId,
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
