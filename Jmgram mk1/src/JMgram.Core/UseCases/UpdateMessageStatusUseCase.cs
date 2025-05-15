

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class UpdateMessageStatusUseCase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateMessageStatusUseCase(IMessageRepository messageRepository, IChatRepository chatRepository, IHttpContextAccessor httpContextAccessor)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<UpdateMessageStatusResponse> Execute(UpdateMessageStatusRequest request, string userId)
        {
            // 1. Валидация входных данных
            if (request == null)
            {
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null.",
                };
            }

            if (request.MessageId <= 0)
            {
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "MessageId must be greater than 0.",
                };
            }

            if (string.IsNullOrEmpty(request.ChatId))
            {
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "ChatId cannot be null or empty.",
                };
            }

            if (!Enum.TryParse(typeof(MessageStatus), request.NewStatus, true, out var parsedStatus))
            {
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Invalid message Status: {request.NewStatus}. It has to be Sent, Delivered, or Read",
                };
            }

            try
            {
                // 2. Получаем сообщение из репозитория
                var message = await _messageRepository.GetMessageById(request.MessageId);

                // 3. Проверяем, существует ли сообщение
                if (message == null)
                {
                    return new UpdateMessageStatusResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Message with Id {request.MessageId} not found.",
                    };
                }

                // 4. Проверяем, что сообщение принадлежит указанному чату
                if (message.ChatId != request.ChatId)
                {
                    return new UpdateMessageStatusResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Message with Id {request.MessageId} does not belong to chat with Id {request.ChatId}.",
                    };
                }

                // 5. Проверяем, является ли пользователь участником чата
                if (!await _chatRepository.IsUserInChat(request.ChatId, userId))
                {
                    return new UpdateMessageStatusResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"User with Id {userId} is not a member of chat with Id {request.ChatId}.",
                    };
                }

                // 6. Проверяем, является ли пользователь *получателем* сообщения
                //    Получателем считаем всех, кто не отправитель сообщения
                if (message.SenderId == userId)
                {
                    return new UpdateMessageStatusResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"User with Id {userId} is the sender of the message, not the recipient.",
                    };
                }

                // 7. Обновляем статус сообщения
                message.Status = (MessageStatus)parsedStatus;

                // 8. Сохраняем изменения в репозитории
                await _messageRepository.Update(message);

                // 9. Формируем успешный ответ
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                // 10. Обрабатываем ошибку
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while updating message status: {ex.Message}",
                };
            }
        }
    }

}
