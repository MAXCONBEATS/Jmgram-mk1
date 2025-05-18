using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class CreateChatUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateChatUseCase(IUserRepository userRepository, IChatRepository chatRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<CreateChatResponse> Execute(CreateChatRequest request)
        {
            // 1. Валидация входных данных
            if (request.Chat == null || string.IsNullOrWhiteSpace(request.Chat.Name) || request.Phones == null || request.Phones.Count() == 0)
            {
                return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные" };
            }

            // 2. Получение UserId создателя чата из Claims
            var creatorUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(creatorUserId))
            {
                return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Не удалось получить ID пользователя из Claims" };
            }

            // 3. Создание чата
            var chat = new Chat
            {
                Name = request.Chat.Name,
                CreatedAt = DateTime.UtcNow,
                CreatorUserId = creatorUserId
            };

            // 4. Создание чата в БД
            var createdChat = await _chatRepository.CreateChat(chat);

            // 5. Получение списка пользователей по телефонам
            var users = await _userRepository.GetByPhones(request.Phones);

            // 6. Проверка, что все пользователи найдены
            if (users == null || users.Count() != request.Phones.Count())
            {
                return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Один или несколько пользователей не найдены" };
            }

            // 7. Добавление создателя чата в чат
            var creatorChatUser = new ChatUser
            {
                ChatId = createdChat.Id,
                UserId = creatorUserId,
                JoinedAt = DateTime.UtcNow
            };
            await _chatRepository.AddUserToChat(creatorChatUser);

            // 8. Добавление пользователей в чат
            foreach (var user in users)
            {
                if (user != null)
                {
                    var chatUser = new ChatUser
                    {
                        ChatId = createdChat.Id,
                        UserId = user.Id,
                        JoinedAt = DateTime.UtcNow
                    };

                    await _chatRepository.AddUserToChat(chatUser);
                }
            }

            // 9. Преобразование в DTO
            var chatDto = new ChatDto
            {
                Name = createdChat.Name,
            };

            // 10. Вернуть результат
            return new CreateChatResponse { IsSuccess = true, Chat = chatDto };
        }
    }
}