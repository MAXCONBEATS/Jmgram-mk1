

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

        public async Task<AddUserToChatResponse> Execute(AddUserToChatRequest request, string invitingUserId) // Принимаем userId
        {
            // 1. Проверить входные данные
            if (request.PhoneNumbers == null || request.PhoneNumbers.Count == 0 || string.IsNullOrEmpty(request.ChatId) || string.IsNullOrEmpty(invitingUserId))
            {
                return new AddUserToChatResponse { IsSuccess = false, Message = "Неверные PhoneNumbers, ChatId или InvitingUserId" };
            }

            // 2. Проверить, что приглашающий пользователь является участником чата
            if (!await _chatRepository.IsUserInChat(request.ChatId, invitingUserId))
            {
                return new AddUserToChatResponse { IsSuccess = false, Message = "Приглашать в чат могут только участники чата" };
            }

            // 3. Получить пользователей по номерам телефона
            var users = await _userRepository.GetByPhones(request.PhoneNumbers);

            if (users == null || users.Count == 0)
            {
                return new AddUserToChatResponse { IsSuccess = false, Message = "Пользователи с указанными номерами телефонов не найдены" };
            }

            // 4. Проверить, что чат существует
            var chat = await _chatRepository.GetById(request.ChatId);

            if (chat == null)
            {
                return new AddUserToChatResponse { IsSuccess = false, Message = "Чат не найден" };
            }

            // 5. Добавить пользователей в чат
            List<ChatUserDto> addedUsers = new List<ChatUserDto>();
            foreach (var user in users)
            {
                var chatUser = new ChatUser
                {
                    ChatId = request.ChatId,
                    UserId = user.Id,
                    JoinedAt = DateTime.UtcNow
                };

                await _chatRepository.AddUserToChat(chatUser);

                // Сконвертировать в DTO
                var chatUserDto = new ChatUserDto
                {
                    ChatId = chatUser.ChatId,
                    UserId = chatUser.UserId,
                    JoinedAt = chatUser.JoinedAt
                };
                addedUsers.Add(chatUserDto);
            }

            // 6. Вернуть результат
            return new AddUserToChatResponse { IsSuccess = true, ChatUsers = addedUsers, Message = "Пользователи успешно добавлены в чат" };
        }
    }
}