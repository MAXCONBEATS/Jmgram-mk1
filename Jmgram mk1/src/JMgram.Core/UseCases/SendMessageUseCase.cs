
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

        public async Task<SendMessageResponse> Execute(SendMessageRequest request)
        {
            // 1. Валидация входных данных
            if (request == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null.",
                    Message = null
                };
            }

            if (request.Message == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Message in request cannot be null.",
                    Message = null
                };
            }
            if (string.IsNullOrWhiteSpace(request.Message.Text))
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Message text cannot be empty.",
                    Message = null
                };
            }
            // 2. Проверка существования Chat и Sender
            var chat = await _chatRepository.GetById(request.Message.ChatId);
            if (chat == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Chat with Id {request.Message.ChatId} does not exist.",
                    Message = null
                };
            }

            var sender = await _userRepository.GetById(request.Message.SenderId);
            if (sender == null)
            {
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"User with Id {request.Message.SenderId} does not exist.",
                    Message = null
                };
            }

            try
            {
                // 3. Преобразование MessageDto в Message Entity
                var messageEntity = new Message
                {
                    ChatId = request.Message.ChatId.ToString(),
                    SenderId = request.Message.SenderId.ToString(),
                    Text = request.Message.Text,
                    Timestamp = DateTime.UtcNow // или request.Message.Timestamp, если нужно сохранить время отправки, указанное клиентом
                };

                // 4. Добавление сообщения в базу данных
                messageEntity.Id = await _messageRepository.Add(messageEntity);


                // 5. Преобразование Message Entity в MessageDto для ответа
                var messageDto = new MessageDto
                {
                    Id = messageEntity.Id,
                    ChatId = messageEntity.ChatId,
                    SenderId = messageEntity.SenderId,
                    Text = messageEntity.Text,
                    Timestamp = messageEntity.Timestamp
                };

                // 6. Формирование успешного ответа
                return new SendMessageResponse
                {
                    IsSuccess = true,
                    Message = messageDto
                };
            }
            catch (Exception ex)
            {
                // 7. Обработка ошибок
                return new SendMessageResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while sending message: {ex.Message}",
                    Message = null
                };
            }
        }

    }

}
