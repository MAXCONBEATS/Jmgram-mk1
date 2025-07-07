using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Services
{
    public interface IMessageValidationService
    {
        Task<bool> CanUserSendMessage(string userId, string chatId);
    }

    public class MessageValidationService : IMessageValidationService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<MessageValidationService> _logger;

        public MessageValidationService(
            IChatRepository chatRepository,
            ILogger<MessageValidationService> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        public async Task<bool> CanUserSendMessage(string userId, string chatId)
        {
            try
            {
                var chat = await _chatRepository.GetById(chatId);
                if (chat == null)
                {
                    _logger.LogWarning($"Chat not found: {chatId}");
                    return false;
                }

                switch (chat.ChatType)
                {
                    case ChatType.Channel:
                        var isCreator = chat.CreatorUserId == userId;
                        _logger.LogInformation($"Channel message check - User: {userId}, Creator: {chat.CreatorUserId}, CanSend: {isCreator}");
                        return isCreator;

                    case ChatType.Group:
                        var isParticipant = await _chatRepository.IsUserInChat(chatId, userId);
                        _logger.LogInformation($"Group message check - User: {userId}, IsParticipant: {isParticipant}");
                        return isParticipant;

                    case ChatType.Private:
                        var isPrivateParticipant = await _chatRepository.IsUserInChat(chatId, userId);
                        return isPrivateParticipant;

                    default:
                        _logger.LogWarning($"Unknown chat type: {chat.ChatType}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking message permissions for user {userId} in chat {chatId}");
                return false;
            }
        }
    }

}
