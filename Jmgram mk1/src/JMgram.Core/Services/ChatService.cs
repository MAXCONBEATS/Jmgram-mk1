using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Services
{
    public interface IChatService
    {
        Task<List<ChatDto>> GetChatsForUser(string userId);
        Task SetChatNameForUser(string userId, string chatId, string chatName);
        Task<string> GetChatNameForUser(string userId, string chatId);
    }
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<List<ChatDto>> GetChatsForUser(string userId)
        {
            var chats = await _chatRepository.GetUserChats(userId);
            var chatDtos = new List<ChatDto>();

            foreach (var chat in chats)
            {
                var chatName = await _chatRepository.GetChatNameForUser(userId, chat.Id);
                chatDtos.Add(new ChatDto
                {
                    ChatId = chat.Id,
                    Name = chatName,
                });
            }

            return chatDtos;
        }
        public async Task<string> GetChatNameForUser(string userId, string chatId)
        {
            return await _chatRepository.GetChatNameForUser(userId, chatId);
        }
        public async Task SetChatNameForUser(string userId, string chatId, string chatName)
        {
            await _chatRepository.SetChatNameForUser(userId, chatId, chatName);
        }
    }

}
