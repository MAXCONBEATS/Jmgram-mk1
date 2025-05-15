

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class AddUserToChatUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;

        public AddUserToChatUseCase(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        public async Task<AddUserToChatResponse> Execute(AddUserToChatRequest request)
        {
            // 1. Проверить входные данные
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.ChatId))
            {
                return new AddUserToChatResponse { IsSuccess = false, Message = "Неверные UserId или ChatId" };
            }

            // 2. Проверить, что пользователь и чат существуют
            var user = await _userRepository.GetById(request.UserId);
            var chat = await _chatRepository.GetById(request.ChatId);

            if (user == null || chat == null)
            {
                return new AddUserToChatResponse { IsSuccess = false, Message = "Пользователь или чат не найдены" };
            }

            // 3. Создать запись ChatUser
            var chatUser = new ChatUser
            {
                ChatId = request.ChatId,
                UserId = request.UserId,
                JoinedAt = DateTime.UtcNow
            };

            // 4. Добавить пользователя в чат
            await _chatRepository.AddUserToChat(chatUser);

            // 5. Сконвертировать в DTO
            var chatUserDto = new ChatUserDto
            {
                ChatId = chatUser.ChatId,
                UserId = chatUser.UserId,
                JoinedAt = chatUser.JoinedAt
            };

            // 6. Вернуть результат
            return new AddUserToChatResponse { IsSuccess = true, ChatUser = chatUserDto };
        }
    }
}