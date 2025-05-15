

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetChatListUseCase
    {
        private readonly IChatRepository _chatRepository;

        public GetChatListUseCase(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        public async Task<GetChatListResponse> Execute(GetChatListRequest request)
        {
            // 1. Проверить входные данные
            if (string.IsNullOrEmpty(request.ChatId))
            {
                return new GetChatListResponse { IsSuccess = false, ErrorMessage = "ChatId cannot be null or empty.", ChatUsers = new List<ChatDto>() };
            }

            // 2. Получаем все записи ChatUser по ChatId
            var chatUsers = await _chatRepository.GetChatUsers(request.ChatId);

            // 3. Если чат не найден (нет записей ChatUser), вернуть пустой список или ошибку (в зависимости от логики)
            if (chatUsers == null || chatUsers.Count == 0) // Проверка на null и пустой список
            {
                return new GetChatListResponse { IsSuccess = true, ChatUsers = new List<ChatDto>() }; // Или вернуть ошибку:  IsSuccess = false, ErrorMessage = "Чат не найден"
            }

            // 4. Преобразуем ChatUser в ChatDto
            var chatDtos = chatUsers.Select(cu => new ChatDto
            {
                Name = cu.Chat.Name,
            }).ToList();

            // 5. Вернуть результат
            return new GetChatListResponse { IsSuccess = true, ChatUsers = chatDtos };
        }
    }

}
