
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetChatMessagesUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;

        public GetChatMessagesUseCase(IChatRepository chatRepository, IMessageRepository messageRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        }

        public async Task<GetChatMessagesResponse> Execute(GetChatMessagesRequest request)
        {
            // 1. Проверить входные данные
            if (request.ChatId <= 0 || request.PageNumber <= 0 || request.PageSize <= 0)
            {
                return new GetChatMessagesResponse
                {
                    Chat = new List<MessageDto>(),
                    TotalMessages = 0,
                    TotalPages = 0
                }; // Или выбросить ArgumentException
            }

            // 2. Получить общее количество сообщений
            var totalMessages = await _messageRepository.GetTotalMessageCount(request.ChatId);

            // 3. Рассчитать общее количество страниц
            var totalPages = (int)Math.Ceiling((double)totalMessages / request.PageSize);

            // 4. Получить сообщения для текущей страницы
            var messages = await _messageRepository.GetMessagesForChat(request.ChatId, request.PageNumber, request.PageSize);

            // 5. Преобразовать сообщения в DTO
            var messageDtos = messages.Select(m => new MessageDto
            {
                ChatId = m.ChatId,
                Text = m.Text,
                Timestamp = m.Timestamp
            }).ToList();

            // 6. Вернуть результат
            return new GetChatMessagesResponse
            {
                Chat = messageDtos,
                TotalMessages = totalMessages,
                TotalPages = totalPages
            };
        }
    }

}
