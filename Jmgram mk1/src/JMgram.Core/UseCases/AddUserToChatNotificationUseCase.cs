using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class AddUserToChatNotificationUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        private readonly SendNotificationUseCase _sendNotificationUseCase;
        private readonly ILogger<AddUserToChatNotificationUseCase> _logger;

        public AddUserToChatNotificationUseCase(IChatRepository chatRepository, IUserRepository userRepository, SendNotificationUseCase sendNotificationUseCase, ILogger<AddUserToChatNotificationUseCase> logger)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));

            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AddUserToChatResponse> Execute(string chatId, string userId, string inviterUserId)
        {
            try
            {
                _logger.LogInformation($"AddUserToChatNotificationUseCase.Execute: Adding user {userId} to chat {chatId} by {inviterUserId}");
                // Добавьте логирование для проверки значения chatId
                _logger.LogInformation($"ChatId value: {chatId}");

                // 1. Проверка существования чата
                if (!await _chatRepository.ChatExists(chatId))
                {
                    return new AddUserToChatResponse { IsSuccess = false, Message = $"Chat with id {chatId} does not exist." };
                }

                // 2. Проверка существования пользователя
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    return new AddUserToChatResponse { IsSuccess = false, Message = $"User with id {userId} does not exist." };
                }

                // 3. Проверка, что пользователь еще не в чате
                if (await _chatRepository.IsUserInChat(chatId, userId))
                {
                    return new AddUserToChatResponse { IsSuccess = false, Message = $"User with id {userId} is already in chat {chatId}." };
                }

                // 4. Отправка уведомления о приглашении в чат
                var notificationDto = new NotificationDto
                {
                    UserId = userId, // Id пользователя, которого приглашают
                    Message = $"Вас пригласил в чат {chatId} пользователь {inviterUserId}. Принять или отклонить?", // Сообщение
                    Timestamp = DateTime.UtcNow,
                    IsRead = false,
                    NotificationType = NotificationType.ChatInvite,
                    ChatId = chatId // Установите ChatId в NotificationDto
                };
                var response = await _sendNotificationUseCase.Execute(notificationDto, inviterUserId);

                if (!response.IsSuccess)
                {
                    _logger.LogError($"AddUserToChatUseCase.Execute: Error sending notification: {response.ErrorMessage}");
                    return new AddUserToChatResponse { IsSuccess = false, Message = $"User added to chat, but failed to send notification: {response.ErrorMessage}" };
                }

                _logger.LogInformation($"AddUserToChatUseCase.Execute: User {userId} added to chat {chatId} successfully.");

                return new AddUserToChatResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddUserToChatUseCase.Execute: An error occurred while adding user to chat: {ex.Message}");
                return new AddUserToChatResponse { IsSuccess = false, Message = $"An error occurred while adding user to chat: {ex.Message}" };
            }

        }
    }
}
