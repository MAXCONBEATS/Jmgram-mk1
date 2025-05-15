using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class CreateChatUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;

        public CreateChatUseCase(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        public async Task<CreateChatResponse> Execute(CreateChatRequest request)
        {
            // 1. Валидация входных данных
            if (request.Chat == null || string.IsNullOrWhiteSpace(request.Chat.Name) || request.Users == null || request.Users.Count() == 0) // Используем Count() как метод
            {
                return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные" };
            }

            // 2. Преобразование ChatDto в Chat
            var chat = new Chat
            {
                Name = request.Chat.Name,
                CreatedAt = DateTime.UtcNow,
            };

            // 3. Создание чата в БД
            var createdChat = await _chatRepository.CreateChat(chat);

            // 4. Получение списка пользователей по ID
            var userIds = request.Users.Select(u => u.Id).ToList(); // Получаем список userIds (string)
            var users = await _userRepository.GetByIds(userIds); // Передаем список string

            // 5. Проверка, что пользователи найдены
            if (users == null || users.Count() != request.Users.Count()) // Используем Count() как метод
            {
                return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Один или несколько пользователей не найдены" };
            }

            // 6. Добавление пользователей в чат
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

            // 7. Преобразование в DTO
            var chatDto = new ChatDto
            {
                ChatId = createdChat.Id,
                Name = createdChat.Name,
                CreatedAt = createdChat.CreatedAt
            };

            // 8. Вернуть результат
            return new CreateChatResponse { IsSuccess = true, Chat = chatDto };
        }
    }
}