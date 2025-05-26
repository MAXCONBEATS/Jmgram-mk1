

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class RespondToChatInviteUseCase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<RespondToChatInviteUseCase> _logger;

        public RespondToChatInviteUseCase(INotificationRepository notificationRepository, IChatRepository chatRepository, ILogger<RespondToChatInviteUseCase> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RespondToChatInviteResponse> Execute(int notificationId, string userId, bool accepted)
        {
            try
            {
                _logger.LogInformation($"RespondToChatInviteUseCase.Execute: User {userId} responding to invite {notificationId} with accepted = {accepted}");
                var notification = await _notificationRepository.GetNotificationById(notificationId);
                if (notification == null)
                {
                    return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"Notification with id {notificationId} not found." };
                }
                if (notification.UserId != userId)
                {
                    return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"Notification with id {notificationId} is not for user {userId}." };
                }
                if (notification is not ChatInviteNotification chatInviteNotification)
                {
                    return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"Notification with id {notificationId} is not a chat invite." };
                }
                string chatId = chatInviteNotification.ChatId;
                if (string.IsNullOrEmpty(chatId))
                {
                    return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"ChatId is empty" };
                }
                var chat = await _chatRepository.GetById(chatId);
                if (chat == null)
                {
                    return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"Chat with id {chatId} not found." };
                }
                if (accepted)
                {
                    var chatUser = new ChatUser
                    {
                        ChatId = chat.Id,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow
                    };
                    await _chatRepository.AddUserToChat(chatUser);
                    _logger.LogInformation($"RespondToChatInviteUseCase.Execute: User {userId} accepted invite to chat {chatId}.");
                }
                else
                {
                    _logger.LogInformation($"RespondToChatInviteUseCase.Execute: User {userId} declined invite to chat {chatId}.");
                }
                await _notificationRepository.DeleteNotification(notificationId);
                _logger.LogInformation($"RespondToChatInviteUseCase.Execute: User {userId} responded to invite {notificationId} successfully.");
                return new RespondToChatInviteResponse { IsSuccess = true, SuccessMessage = accepted ? "Вы приняли приглашение в чат." : "Вы отклонили приглашение в чат." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"RespondToChatInviteUseCase.Execute: An error occurred while responding to chat invite: {ex.Message}");
                return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"An error occurred while responding to chat invite: {ex.Message}" };
            }
        }

    }
}
