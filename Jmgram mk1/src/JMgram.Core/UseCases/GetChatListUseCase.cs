

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

        public async Task<GetChatListResponse> Execute(GetChatListRequest request, string userId)
        {
            // 1. Проверить входные данные
            if (string.IsNullOrEmpty(request.ChatId) || string.IsNullOrEmpty(userId))
            {
                return new GetChatListResponse { IsSuccess = false, ErrorMessage = "ChatId и UserId не могут быть пустыми", Users = new List<UserDto>() };
            }

            // 2. Проверить, что пользователь является участником чата
            if (!await _chatRepository.IsUserInChat(request.ChatId, userId))
            {
                return new GetChatListResponse { IsSuccess = false, ErrorMessage = "Вы не являетесь участником этого чата", Users = new List<UserDto>() };
            }

            // 3. Получаем все записи ChatUser по ChatId
            var chatUsers = await _chatRepository.GetChatUsers(request.ChatId);

            // 4. Если чат не найден (нет записей ChatUser), вернуть пустой список или ошибку
            if (chatUsers == null || chatUsers.Count == 0)
            {
                return new GetChatListResponse { IsSuccess = true, Users = new List<UserDto>() };
            }

            // 5. Преобразуем ChatUser в UserDto
            var userDtos = chatUsers.Select(cu => new UserDto
            {
                Id = cu.UserId,
                Phone = cu.User.PhoneNumber
            }).ToList();

            // 6. Вернуть результат
            return new GetChatListResponse { IsSuccess = true, Users = userDtos };
        }
    }

}
