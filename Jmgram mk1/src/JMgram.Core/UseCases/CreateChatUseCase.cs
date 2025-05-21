using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Jmgram_mk1.src.JMgram.Core.Storage;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class CreateChatUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateChatUseCase> _logger;

        public CreateChatUseCase(IUserRepository userRepository, IChatRepository chatRepository, IHttpContextAccessor httpContextAccessor, ILogger<CreateChatUseCase> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CreateChatResponse> Execute(CreateChatRequest request)
        {
            _logger.LogInformation("CreateChatUseCase.Execute: Starting execution");
            try
            {
                // 1. Валидация входных данных
                if (request.Chat == null || string.IsNullOrWhiteSpace(request.Chat.Name) || request.Phones == null || request.Phones.Count == 0)
                {
                    _logger.LogError("CreateChatUseCase.Execute: Invalid input data");
                    return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные" };
                }

                _logger.LogInformation($"CreateChatUseCase.Execute: Chat name = {request.Chat.Name}, Phones = {string.Join(", ", request.Phones)}");

                // 2. Получение UserId создателя чата из Claims
                var creatorUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(creatorUserId))
                {
                    _logger.LogError("CreateChatUseCase.Execute: Could not get UserId from Claims");
                    return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Не удалось получить ID пользователя из Claims" };
                }

                _logger.LogInformation($"CreateChatUseCase.Execute: CreatorUserId = {creatorUserId}");

                // 3. Создание чата
                var chat = new Chat
                {
                    Name = request.Chat.Name,
                    CreatedAt = DateTime.UtcNow,
                    CreatorUserId = creatorUserId
                };

                _logger.LogInformation($"CreateChatUseCase.Execute: About to create chat in database");
                // 4. Создание чата в БД
                var createdChat = await _chatRepository.CreateChat(chat);
                _logger.LogInformation($"CreateChatUseCase.Execute: Chat created with ID = {createdChat.Id}");

                // 5. Получение списка пользователей по телефонам
                _logger.LogInformation($"CreateChatUseCase.Execute: Getting users by phones: {string.Join(", ", request.Phones)}");
                var users = await _userRepository.GetByPhones(request.Phones);

                // 6. Проверка, что все пользователи найдены
                if (users == null || users.Count() != request.Phones.Count())
                {
                    _logger.LogWarning($"CreateChatUseCase.Execute: Not all users were found for phones: {string.Join(", ", request.Phones)}");
                    // You can choose to return an error or proceed with the found users
                }

                _logger.LogInformation($"CreateChatUseCase.Execute: Found {users?.Count() ?? 0} users");

                // 7. Добавление создателя чата в чат
                var creatorChatUser = new ChatUser
                {
                    ChatId = createdChat.Id,
                    UserId = creatorUserId,
                    JoinedAt = DateTime.UtcNow
                };
                _logger.LogInformation($"CreateChatUseCase.Execute: Adding creator {creatorUserId} to chat {createdChat.Id}");
                await _chatRepository.AddUserToChat(creatorChatUser);
                _logger.LogInformation($"CreateChatUseCase.Execute: Creator added to chat");

                // 8. Добавление пользователей в чат
                if (users != null)
                {
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

                            _logger.LogInformation($"CreateChatUseCase.Execute: Adding user {user.Id} to chat {createdChat.Id}");
                            await _chatRepository.AddUserToChat(chatUser);
                            _logger.LogInformation($"CreateChatUseCase.Execute: User {user.Id} added to chat");
                        }
                    }
                }

                // 9. Преобразование в DTO
                var chatDto = new ChatDto
                {
                    Name = createdChat.Name
                };

                // 10. Вернуть результат
                _logger.LogInformation("CreateChatUseCase.Execute: Successfully completed");
                return new CreateChatResponse { IsSuccess = true, Chat = chatDto };
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateChatUseCase.Execute: An error occurred: {ex.Message}");
                return new CreateChatResponse { IsSuccess = false, ErrorMessage = "An error occurred while creating the chat." };
            }
        }
    }
}