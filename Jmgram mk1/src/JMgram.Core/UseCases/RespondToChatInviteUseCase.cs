

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
        private readonly IChatInvationRepository _chatInvationRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<RespondToChatInviteUseCase> _logger;

        public RespondToChatInviteUseCase(INotificationRepository notificationRepository, IChatRepository chatRepository, ILogger<RespondToChatInviteUseCase> logger, IChatInvationRepository chatInvationRepository)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatInvationRepository = chatInvationRepository;
        }

        public async Task<RespondToChatInviteResponse> Execute(Guid chatInvitationId, string userId, bool accepted)
        {
            try
            {
                _logger.LogInformation($"RespondToChatInviteUseCase.Execute: User {userId} responding to invite {chatInvitationId} with accepted = {accepted}");
                var chatInvitation = await _chatInvationRepository.GetChatInvitationById(chatInvitationId);
                if (chatInvitation == null)
                {
                    return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"ChatInvitation with id {chatInvitation} not found." };
                }
                if (chatInvitation.RecipientUserId != userId)
                {
                    return new RespondToChatInviteResponse { IsSuccess = false, ErrorMessage = $"Notification with id {chatInvitation} is not for user {userId}." };
                }
                string chatId = chatInvitation.ChatId;
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
                        ChatName = chat.Name,
                        JoinedAt = DateTime.UtcNow
                    };
                    await _chatRepository.AddUserToChat(chatUser);
                    chatInvitation.Status = ChatInvationStatus.Accepted;
                    _logger.LogInformation($"RespondToChatInviteUseCase.Execute: User {userId} accepted invite to chat {chatId}.");
                }
                else
                {
                    _logger.LogInformation($"RespondToChatInviteUseCase.Execute: User {userId} declined invite to chat {chatId}.");
                    chatInvitation.Status = ChatInvationStatus.Rejected;
                }
                await _chatInvationRepository.UpdateChatInvitation(chatInvitation);      
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
