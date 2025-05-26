using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public interface IGetLastChatMessageUseCase
    {
        Task<LastChatMessageDto> Execute(string chatId, string userId);
    }
    public class GetLastChatMessageUseCase : IGetLastChatMessageUseCase
    {
        private readonly IChatRepository _chatRepository;

        public GetLastChatMessageUseCase(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        public async Task<LastChatMessageDto> Execute(string chatId, string userId)
        {

            Guid chatIdGuid;
            if (!Guid.TryParse(chatId, out chatIdGuid))
            {
                throw new ArgumentException($"Invalid ChatId format: {chatId}");
            }

            var lastChatMessage = await _chatRepository.GetLastChatMessage(chatId);

            if (lastChatMessage == null)
            {
                return null;
            }

            return lastChatMessage;
        }
    }
}