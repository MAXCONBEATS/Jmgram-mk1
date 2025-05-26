

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

        public async Task<GetNotificationListResponse> Execute(string userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetNotificationsByUserId(userId);
                var notificationDtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Message = n.Message,
                    Timestamp = n.Timestamp,
                    IsRead = n.IsRead,
                    NotificationType = n.NotificationType,
                    ChatId = n.ChatId
                }).ToList();

                return new GetNotificationListResponse
                {
                    IsSuccess = true,
                    Notifications = notificationDtos
                };
            }
            catch (Exception ex)
            {
                return new GetNotificationListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Notifications = null
                };
            }
        }
    }
}
