using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class CreateChatInvitationUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatInvationRepository _chatInvationRepository;
        private readonly IUserRepository _userRepository;
        private readonly SendNotificationUseCase _sendNotificationUseCase;
        private readonly ILogger<CreateContactRequestUseCase> _logger;
        public CreateChatInvitationUseCase(IChatRepository chatRepository, IUserRepository userRepository, ILogger<CreateContactRequestUseCase> logger, IChatInvationRepository chatInvationRepository, SendNotificationUseCase sendNotificationUseCase)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _logger = logger;
            _chatInvationRepository = chatInvationRepository;
            _sendNotificationUseCase = sendNotificationUseCase;
        }
        public async Task<CreateChatInvitationResponse> Execute(string chatId, string senderUserId, string recipientUserId)
        {
            var existingChat = await _chatRepository.ChatExists(chatId);
            if (existingChat == false)
            {
                _logger.LogWarning("CreateChatInvitationUseCase.Execute: Chat not exists");
                return new CreateChatInvitationResponse { IsSuccess = false, ErrorMessage = "Указаного чата не существует." };
            }
            if (string.IsNullOrEmpty(recipientUserId))
            {
                _logger.LogWarning("CreateChatInvitationUseCase.Execute: Invalid userId");
                return new CreateChatInvitationResponse { IsSuccess = false, ErrorMessage = "Неверный идентификатор пользователя." };
            }

            if (senderUserId == recipientUserId)
            {
                _logger.LogWarning("CreateChatInvitationUseCase.Execute: User already in chat.");
                return new CreateChatInvitationResponse { IsSuccess = false, ErrorMessage = "Вы уже в этом чате." };
            }
            var chatInvitation = new ChatInvitation
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                Status = ChatInvationStatus.Pending
            };
            var existingChatInvitation = await _chatInvationRepository.GetChatInvitation(chatId, senderUserId, recipientUserId);
            if (existingChatInvitation != null)
            {
                _logger.LogWarning($"CreateChatInvitationUseCase.Execute: Chat Invitation already exists between {senderUserId} and {recipientUserId}.");
                return new CreateChatInvitationResponse { IsSuccess = false, ErrorMessage = "Приглашение уже отправлено." };
            }
            _logger.LogInformation("CreateContactRequestUseCase.Execute: ContactRequest created.");
            try
            {
                await _chatInvationRepository.AddChatInvitation(chatInvitation);
                _logger.LogInformation("CreateChatInvitationUseCase.Execute: Chat Invitation saved to database.");
                var senderUser = await _userRepository.GetById(senderUserId);
                string senderName = senderUser?.FirstName ?? "Неизвестный пользователь";
                var chat = await _chatRepository.GetById(chatId);
                string chatName = chat?.Name;
                NotificationDto notificationDto = new NotificationDto
                {
                    UserId = recipientUserId,
                    NotificationType = NotificationType.ChatInvite,
                    Message = $"Приглашение в чат {chatName} от {senderName}",
                    Timestamp = DateTime.UtcNow,
                    IsRead = false,
                    ChatId = chatId
                };
                var sendNotificationResponse = await _sendNotificationUseCase.Execute(notificationDto, senderUserId);
                _logger.LogInformation("CreateChatInvitationUseCase.Execute: Notification sent.");

                _logger.LogInformation("CreateChatInvitationUseCase.Execute: Execution completed successfully.");
                return new CreateChatInvitationResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateChatInvitationUseCase.Execute: Error during execution: {ex.Message}. Inner exception: {ex.InnerException?.Message}");
                return new CreateChatInvitationResponse { IsSuccess = false, ErrorMessage = $"Ошибка при создании запроса: {ex.Message}" };
            }
        }
    }
}
