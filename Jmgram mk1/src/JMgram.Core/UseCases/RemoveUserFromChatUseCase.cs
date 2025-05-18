using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class RemoveUserFromChatUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<RemoveUserFromChatUseCase> _logger;

        public RemoveUserFromChatUseCase(IChatRepository chatRepository, ILogger<RemoveUserFromChatUseCase> logger)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RemoveUserFromChatResponse> Execute(string chatId, string userId)
        {
            try
            {
                _logger.LogInformation($"RemoveUserFromChatUseCase.Execute: Removing user {userId} from chat {chatId}");

                // 1. Проверка существования чата
                if (!await _chatRepository.ChatExists(chatId))
                {
                    return new RemoveUserFromChatResponse { IsSuccess = false, ErrorMessage = $"Chat with id {chatId} does not exist." };
                }

                // 2. Проверка, что пользователь состоит в чате
                if (!await _chatRepository.IsUserInChat(chatId, userId))
                {
                    return new RemoveUserFromChatResponse { IsSuccess = false, ErrorMessage = $"User with id {userId} is not in chat {chatId}." };
                }

                // 3. Удаление пользователя из чата
                await _chatRepository.RemoveUserFromChat(chatId, userId);

                _logger.LogInformation($"RemoveUserFromChatUseCase.Execute: User {userId} removed from chat {chatId} successfully.");

                return new RemoveUserFromChatResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"RemoveUserFromChatUseCase.Execute: An error occurred while removing user from chat: {ex.Message}");
                return new RemoveUserFromChatResponse { IsSuccess = false, ErrorMessage = $"An error occurred while removing user from chat: {ex.Message}" };
            }
        }
    }
}
