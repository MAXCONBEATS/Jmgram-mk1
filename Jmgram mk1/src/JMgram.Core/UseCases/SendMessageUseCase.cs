
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;
using System.Net;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class SendMessageUseCase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly SendNotificationUseCase _sendNotificationUseCase;
        private readonly ILogger<SendMessageUseCase> _logger;

        public SendMessageUseCase(IMessageRepository messageRepository, IChatRepository chatRepository, IUserRepository userRepository, SendNotificationUseCase sendNotificationUseCase, ILogger<SendMessageUseCase> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendMessageResponse> Execute(SendMessageRequest request, string senderUserId)
        {
            // 1. Валидация входных данных
            if (request == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null.",
                };
            }

            if (request.Message == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Message in request cannot be null.",
                };
            }
            if (string.IsNullOrWhiteSpace(request.Message.Text))
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Message text cannot be empty.",
                };
            }

            // 2. Проверка существования Chat
            var chat = await _chatRepository.GetById(request.Message.ChatId, includeChatUsers: true);
            if (chat == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Chat with Id {request.Message.ChatId} does not exist.",
                };
            }

            // 3. Проверить, что отправитель является участником чата
            if (!await _chatRepository.IsUserInChat(request.Message.ChatId, senderUserId))
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"User with Id {senderUserId} is not a member of chat {request.Message.ChatId}.",
                };
            }

            // 4. Получаем пользователя
            var sender = await _userRepository.GetById(senderUserId);
            if (sender == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"User with Id {senderUserId} does not exist.",
                };
            }
            // 5. Получаем имя пользователя
            var senderName = await _userRepository.GetUserFirstNameById(senderUserId);
            if (string.IsNullOrEmpty(senderName))
            {
                _logger.LogError($"SendMessageUseCase.Execute: Sender with ID = {senderUserId} not found or FirstName is empty.");
                return new SendMessageResponse { IsSuccess = false, ErrorMessage = $"Пользователь с ID {senderUserId} не найден или у него не указано имя." };
            }

            try
            {
                // 6. Создание Message Entity
                var messageEntity = new Message
                {
                    ChatId = request.Message.ChatId,
                    SenderId = senderUserId, // Используем userId авторизованного пользователя
                    SenderName = senderName,
                    Text = request.Message.Text,
                    Timestamp = request.Message.Timestamp // Используем Timestamp из request
                };

                // 7. Добавление сообщения в базу данных
                var id = await _messageRepository.Add(messageEntity);

                // Get the recipient UserId
                var recipientChatUser = chat.ChatUsers.FirstOrDefault(cu => cu.UserId != senderUserId); // Find the other user in the chat

                if (recipientChatUser == null)
                {
                    _logger.LogWarning($"SendMessageUseCase.Execute: No other user found in chat {request.Message.ChatId} besides user {senderUserId}.");
                    return new SendMessageResponse { IsSuccess = true, Id = id }; // Message sent, but no notification sent because there's no other user
                }

                var recipientId = recipientChatUser.UserId;

                // Send notification to the recipient
                var notificationDto = new NotificationDto
                {
                    UserId = recipientId,
                    Message = $"Новое сообщение от {senderName}: {request.Message.Text.Substring(0, Math.Min(request.Message.Text.Length, 50))}", // Truncate message for notification
                    Timestamp = DateTime.UtcNow,
                    IsRead = false,
                    NotificationType = NotificationType.Message
                };

                var notificationResponse = await _sendNotificationUseCase.Execute(notificationDto, senderUserId); // Pass senderUserId

                if (!notificationResponse.IsSuccess)
                {
                    _logger.LogError($"SendMessageUseCase.Execute: Error sending notification: {notificationResponse.ErrorMessage}");
                    return new SendMessageResponse { IsSuccess = false, ErrorMessage = $"Message sent, but failed to send notification: {notificationResponse.ErrorMessage}" };
                }

                // 8. Формирование успешного ответа
                return new SendMessageResponse
                {
                    IsSuccess = true,
                    Id = id
                };
            }
            catch (Exception ex)
            {
                // 9. Обработка ошибок
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while sending message: {ex.Message}",
                };
            }
        }
    }
}


