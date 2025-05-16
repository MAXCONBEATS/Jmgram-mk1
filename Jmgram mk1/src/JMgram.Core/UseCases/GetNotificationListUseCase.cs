

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
                // 1. Get notifications by userId from repository
                var notifications = await _notificationRepository.GetNotificationsByUserId(userId);

                // 2. Form successful response
                return new GetNotificationListResponse
                {
                    IsSuccess = true,
                    Notifications = notifications
                };
            }
            catch (Exception ex)
            {
                // 3. Handle errors
                return new GetNotificationListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while getting notification list: {ex.Message}",
                    Notifications = null
                };
            }
        }
    }
}
