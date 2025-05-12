

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
            if (request.ChatId <= 0)
            {
                return new GetChatListResponse { ChatUsers = new List<ChatDto>() }; // Или вернуть ошибку
            }

            // Получаем всех ChatUser по ChatId
            var chatUsers = await _chatRepository.GetChatUsers(request.ChatId);

            // Преобразуем ChatUser в ChatDto
            var chatDtos = chatUsers.Select(cu => new ChatDto
            {
                ChatId = cu.ChatId,
                UserId = cu.UserId,
                UserName = cu.Chat.Name, // Или другое поле для отображения имени
                JoinedAt = cu.JoinedAt
            }).ToList();

            return new GetChatListResponse { ChatUsers = chatDtos };
        }
    }

}
