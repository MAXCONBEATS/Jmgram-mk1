
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;


namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class RemoveUserFromChatUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatInvationRepository _invationRepository;
        private readonly ILogger<RemoveUserFromChatUseCase> _logger;

        public RemoveUserFromChatUseCase(IChatRepository chatRepository, ILogger<RemoveUserFromChatUseCase> logger, IChatInvationRepository invationRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _invationRepository = invationRepository;
        }

        public async Task<RemoveUserFromChatResponse> Execute(string chatId, string userId, string currentUserId)
        {
            var chatInvitation = await _invationRepository.GetChatInvitation(userId);
            
            try
            {
                _logger.LogInformation($"Removing user {userId} from chat {chatId} initiated by {currentUserId}");

                var chat = await _chatRepository.GetById(chatId);
                if (chat == null)
                {
                    return new RemoveUserFromChatResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Chat with id {chatId} does not exist."
                    };
                }

                if (userId == currentUserId)
                {
                    return new RemoveUserFromChatResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Вы не можете удалить себя из чата."
                    };
                }

                if (chat.CreatorUserId != currentUserId)
                {
                    return new RemoveUserFromChatResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Только создатель чата может удалять участников."
                    };
                }

                if (!await _chatRepository.IsUserInChat(chatId, userId))
                {
                    return new RemoveUserFromChatResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Пользователь {userId} не состоит в этом чате."
                    };
                }
                if (chatInvitation != null)
                {
                    await _invationRepository.DeleteChatInvitation(chatInvitation);
                }
                await _chatRepository.RemoveUserFromChat(chatId, userId);              
                _logger.LogInformation($"User {userId} removed from chat {chatId} by {currentUserId}");
                return new RemoveUserFromChatResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user from chat");
                return new RemoveUserFromChatResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Ошибка при удалении пользователя из чата"
                };
            }
        }
    }

}
