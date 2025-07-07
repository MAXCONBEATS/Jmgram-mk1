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
                if (request.Chat == null || string.IsNullOrWhiteSpace(request.Chat.Name) || request.Phones == null || request.Phones.Count == 0)
                {
                    _logger.LogError("CreateChatUseCase.Execute: Invalid input data");
                    return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные" };
                }

                _logger.LogInformation($"CreateChatUseCase.Execute: Chat name = {request.Chat.Name}, ChatType = {request.Chat.ChatType}, Phones = {string.Join(", ", request.Phones)}");

                var creatorUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(creatorUserId))
                {
                    _logger.LogError("CreateChatUseCase.Execute: Could not get UserId from Claims");
                    return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Не удалось получить ID пользователя из Claims" };
                }

                _logger.LogInformation($"CreateChatUseCase.Execute: CreatorUserId = {creatorUserId}");

                // ПРЕОБРАЗУЕМ int в enum
                ChatType chatType = (ChatType)request.Chat.ChatType;
                _logger.LogInformation($"CreateChatUseCase.Execute: Converted ChatType = {chatType} ({request.Chat.ChatType})");

                var chat = new Chat
                {
                    Name = request.Chat.Name,
                    CreatedAt = DateTime.UtcNow,
                    CreatorUserId = creatorUserId,
                    ChatType = chatType // ← ИСПОЛЬЗУЕМ преобразованный enum
                };

                var createdChat = await _chatRepository.CreateChat(chat);
                _logger.LogInformation($"CreateChatUseCase.Execute: Chat created with ID = {createdChat.Id}, Type = {createdChat.ChatType}");

                // Остальной код остается без изменений...
                var users = await _userRepository.GetByPhones(request.Phones);
                var contacts = await _userRepository.GetContactsForUser(creatorUserId);
                var contactPhones = contacts.Select(c => c.Phone).ToList();

                if (users == null || users.Count() != request.Phones.Count() || !request.Phones.All(phone => contactPhones.Contains(phone)))
                {
                    _logger.LogWarning($"CreateChatUseCase.Execute: Not all users were found or are not contacts for phones: {string.Join(", ", request.Phones)}");
                    return new CreateChatResponse { IsSuccess = false, ErrorMessage = "Не все пользователи найдены или не являются вашими контактами." };
                }

                // Добавляем создателя
                var creatorChatUser = new ChatUser
                {
                    ChatId = createdChat.Id,
                    UserId = creatorUserId,
                    JoinedAt = DateTime.UtcNow,
                    ChatName = createdChat.Name
                };
                await _chatRepository.AddUserToChat(creatorChatUser);

                // Добавляем остальных пользователей
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
                                JoinedAt = DateTime.UtcNow,
                                ChatName = createdChat.Name
                            };
                            await _chatRepository.AddUserToChat(chatUser);
                        }
                    }
                }

                var chatDto = new ChatDto
                {
                    ChatId = createdChat.Id,
                    Name = createdChat.Name,
                    ChatType = createdChat.ChatType
                };

                _logger.LogInformation($"CreateChatUseCase.Execute: Successfully completed. Created {chatType} with ID {chatDto.ChatId}");
                return new CreateChatResponse { IsSuccess = true, Chat = chatDto };
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateChatUseCase.Execute: An error occurred: {ex.Message}");
                _logger.LogError($"CreateChatUseCase.Execute: Stack trace: {ex.StackTrace}");
                return new CreateChatResponse { IsSuccess = false, ErrorMessage = "An error occurred while creating the chat." };
            }
        }

    }
}