

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetNotificationListUseCase
    {
        private readonly INotificationRepository _notificationRepository;

        public GetNotificationListUseCase(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }

        public async Task<GetNotificationListResponse> Execute(GetNotificationListRequest request)
        {
            // 1. Валидация входных данных
            if (request == null)
            {
                return new GetNotificationListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null.",
                    Notifications = new List<NotificationDto>()
                };
            }

            if (request.UserId <= 0)
            {
                return new GetNotificationListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "UserId must be greater than 0.",
                    Notifications = new List<NotificationDto>()
                };
            }

            try
            {
                // 2. Получение уведомлений из репозитория
                var notifications = await _notificationRepository.GetNotificationsByUserId(request.UserId);

                // 3. Формирование успешного ответа
                return new GetNotificationListResponse
                {
                    IsSuccess = true,
                    Notifications = notifications
                };
            }
            catch (Exception ex)
            {
                // 4. Обработка ошибок
                return new GetNotificationListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while retrieving notifications: {ex.Message}",
                    Notifications = new List<NotificationDto>()
                };
            }
        }
    }
}
