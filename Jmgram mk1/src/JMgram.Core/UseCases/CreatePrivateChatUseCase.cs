using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class CreatePrivateChatUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CreatePrivateChatUseCase> _logger;

        public CreatePrivateChatUseCase(IChatRepository chatRepository, IUserRepository userRepository, ILogger<CreatePrivateChatUseCase> logger)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CreatePrivateChatResponse> Execute(CreatePrivateChatRequest request)
        {
            _logger.LogInformation($"CreatePrivateChatUseCase.Execute: Starting execution for User1Id = {request.UserId1}, User2Id = {request.UserId2}");

            try
            {
                if (string.IsNullOrEmpty(request.UserId1) || string.IsNullOrEmpty(request.UserId2))
                {
                    _logger.LogError("CreatePrivateChatUseCase.Execute: Invalid input data - UserId1 or UserId2 is null or empty.");
                    return new CreatePrivateChatResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные: UserId1 или UserId2 не указаны." };
                }

                if (request.UserId1 == request.UserId2)
                {
                    _logger.LogError("CreatePrivateChatUseCase.Execute: Invalid input data - UserId1 and UserId2 are the same.");
                    return new CreatePrivateChatResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные: UserId1 и UserId2 совпадают." };
                }

                _logger.LogInformation($"CreatePrivateChatUseCase.Execute: Getting User1 name for UserId = {request.UserId1}");
                var user1 = await _userRepository.GetById(request.UserId1);
                if (user1 == null)
                {
                    _logger.LogError($"CreatePrivateChatUseCase.Execute: User1 not found for UserId = {request.UserId1}");
                    return new CreatePrivateChatResponse { IsSuccess = false, ErrorMessage = $"Пользователь с ID {request.UserId1} не найден." };
                }

                _logger.LogInformation($"CreatePrivateChatUseCase.Execute: Getting User2 name for UserId = {request.UserId2}");
                var user2 = await _userRepository.GetById(request.UserId2);
                if (user2 == null)
                {
                    _logger.LogError($"CreatePrivateChatUseCase.Execute: User2 not found for UserId = {request.UserId2}");
                    return new CreatePrivateChatResponse { IsSuccess = false, ErrorMessage = $"Пользователь с ID {request.UserId2} не найден." };
                }

                var chat = new Chat
                {
                    Name = "PrivateChat",
                    CreatorUserId = request.UserId1,
                    CreatedAt = DateTime.UtcNow
                };

                var createdChat = await _chatRepository.CreateChat(chat);
                _logger.LogInformation($"CreatePrivateChatUseCase.Execute: Chat created with ID = {createdChat.Id}");

                await _chatRepository.AddUserToChat(new ChatUser { ChatId = createdChat.Id, UserId = request.UserId1, ChatName = $"Переписка с {user2.FirstName}", JoinedAt = DateTime.UtcNow });
                await _chatRepository.AddUserToChat(new ChatUser { ChatId = createdChat.Id, UserId = request.UserId2, ChatName = $"Переписка с {user1.FirstName}", JoinedAt = DateTime.UtcNow });

                _logger.LogInformation("CreatePrivateChatUseCase.Execute: Successfully completed.");
                return new CreatePrivateChatResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreatePrivateChatUseCase.Execute: An error occurred: {ex.Message}");
                return new CreatePrivateChatResponse { IsSuccess = false, ErrorMessage = $"An error occurred while creating private chat: {ex.Message}" };
            }
        }
    }
}
