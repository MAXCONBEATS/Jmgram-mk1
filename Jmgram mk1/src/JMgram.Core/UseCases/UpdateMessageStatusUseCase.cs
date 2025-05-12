

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class UpdateMessageStatusUseCase
    {
        private readonly IMessageRepository _messageRepository;

        public UpdateMessageStatusUseCase(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        }

        public async Task<UpdateMessageStatusResponse> Execute(UpdateMessageStatusRequest request)
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

                // 4. Обновляем статус сообщения                
                message.Status = (MessageStatus)parsedStatus;

                // 5. Сохраняем изменения в репозитории
                await _messageRepository.Update(message);

                // 6. Формируем успешный ответ
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                // 7. Обрабатываем ошибку
                return new UpdateMessageStatusResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while updating message status: {ex.Message}",
                };
            }
        }
    }


}
