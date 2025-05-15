
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using System.Net;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class SendMessageUseCase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        public SendMessageUseCase(IMessageRepository messageRepository, IChatRepository chatRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<SendMessageResponse> Execute(SendMessageRequest request, string userId)
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
            var chat = await _chatRepository.GetById(request.Message.ChatId);
            if (chat == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Chat with Id {request.Message.ChatId} does not exist.",
                };
            }

            // 3. Проверить, что отправитель является участником чата
            if (!await _chatRepository.IsUserInChat(request.Message.ChatId, userId))
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"User with Id {userId} is not a member of chat {request.Message.ChatId}.",
                };
            }

            // 4. Получаем пользователя
            var sender = await _userRepository.GetById(userId);
            if (sender == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"User with Id {userId} does not exist.",
                };
            }

            try
            {
                // 5. Создание Message Entity
                var messageEntity = new Message
                {
                    ChatId = request.Message.ChatId,
                    SenderId = userId, // Используем userId авторизованного пользователя
                    Text = request.Message.Text,
                    Timestamp = request.Message.Timestamp // Используем Timestamp из request
                };

                // 6. Добавление сообщения в базу данных
                var id = await _messageRepository.Add(messageEntity);

                // 7. Формирование успешного ответа
                return new SendMessageResponse
                {
                    IsSuccess = true,
                    Id = id
                };
            }
            catch (Exception ex)
            {
                // 8. Обработка ошибок
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while sending message: {ex.Message}",
                };
            }
        }
    }

}


