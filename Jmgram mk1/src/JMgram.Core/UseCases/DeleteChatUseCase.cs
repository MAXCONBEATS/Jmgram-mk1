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
    public class DeleteChatUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<DeleteChatUseCase> _logger;

        public DeleteChatUseCase(IChatRepository chatRepository, ILogger<DeleteChatUseCase> logger)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DeleteChatResponse> Execute(string chatId)
        {
            try
            {
                _logger.LogInformation($"DeleteChatUseCase.Execute: Deleting chat {chatId}");

                // 1. Проверка существования чата
                if (!await _chatRepository.ChatExists(chatId))
                {
                    return new DeleteChatResponse { IsSuccess = false, ErrorMessage = $"Chat with id {chatId} does not exist." };
                }

                // 2. Удаление чата
                await _chatRepository.DeleteChat(chatId);

                _logger.LogInformation($"DeleteChatUseCase.Execute: Chat {chatId} deleted successfully.");

                return new DeleteChatResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteChatUseCase.Execute: An error occurred while deleting chat: {ex.Message}");
                return new DeleteChatResponse { IsSuccess = false, ErrorMessage = $"An error occurred while deleting chat: {ex.Message}" };
            }
        }
    }
}
