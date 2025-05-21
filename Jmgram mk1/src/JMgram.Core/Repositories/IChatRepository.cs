using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IChatRepository
    {
        Task<Chat?> GetById(string id, bool includeChatUsers = false);
        Task AddUserToChat(ChatUser chatUser);
        Task<Chat> CreateChat(Chat chat);
        Task<List<ChatUser>> GetChatUsers(string chatId);
        Task<bool> ChatExists(string chatId);
        Task<bool> IsUserInChat(string chatId, string userId);
        Task RemoveUserFromChat(string chatId, string userId);
        Task DeleteChat(string chatId);
        Task<List<Chat>> GetUserChats (string userId);
        Task<LastChatMessageDto> GetLastChatMessage(string chatId);
        Task<Chat> GetChatBetweenUsers(string userId1, string userId2);
    }
    public class ChatRepository : IChatRepository
    {
        private readonly JMgramDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatRepository(JMgramDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Chat?> GetById(string id, bool includeChatUsers = false)
        {
            var query = _dbContext.Chats.Where(c => c.Id == id);

            if (includeChatUsers)
            {
                query = query.Include(c => c.ChatUsers);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task AddUserToChat(ChatUser chatUser)
        {
            _dbContext.ChatUsers.Add(chatUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ChatUser>> GetChatUsers(string chatId)
        {
            return await _dbContext.ChatUsers
                .Where(cu => cu.ChatId == chatId)
                .Include(cu => cu.User)
                .ToListAsync();
        }
        public async Task<Chat> GetChatBetweenUsers(string userId1, string userId2)
        {
            return await _dbContext.Chats
             .Include(c => c.ChatUsers)
             .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId1) && c.ChatUsers.Any(cu => cu.UserId == userId2))
             .FirstOrDefaultAsync();
        }
        public async Task<List<Chat>> GetUserChats(string userId)
        {
            var chats = await _dbContext.Chats
             .Join(
              _dbContext.ChatUsers,
              chat => chat.Id,
              chatUser => chatUser.ChatId,
              (chat, chatUser) => new { Chat = chat, ChatUser = chatUser }
             )
             .Where(joined => joined.ChatUser.UserId == userId)
             .Select(joined => joined.Chat)
             .ToListAsync();

            return chats;
        }
        public async Task<LastChatMessageDto> GetLastChatMessage(string chatId)
        {
            var lastMessage = await _dbContext.Messages
             .Where(m => m.ChatId == chatId)
             .OrderByDescending(m => m.Timestamp)
             .Select(m => new LastChatMessageDto
             {
                 ChatId = m.ChatId,
                 Text = m.Text,
                 SenderId = m.SenderId,
                 SenderName = _dbContext.Users.FirstOrDefault(u => u.Id == m.SenderId).FirstName,
                 Timestamp = m.Timestamp
             })
             .FirstOrDefaultAsync();

            return lastMessage;
        }

        public async Task<Chat> CreateChat(Chat chat)
        {
            // Get the current UserId
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Set the CreatorUserId if available
            if (!string.IsNullOrEmpty(userId))
            {
                chat.CreatorUserId = userId;
            }

            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();
            return chat;
        }

        public async Task<bool> ChatExists(string chatId)
        {
            return await _dbContext.Chats.AnyAsync(c => c.Id == chatId);
        }
        public async Task<bool> IsUserInChat(string chatId, string userId)
        {
            return await _dbContext.ChatUsers.AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
        }
        public async Task RemoveUserFromChat(string chatId, string userId)
        {
            var chatUser = await _dbContext.ChatUsers
             .FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);

            if (chatUser != null)
            {
                _dbContext.ChatUsers.Remove(chatUser);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task DeleteChat(string chatId)
        {
            var chat = await _dbContext.Chats.FindAsync(chatId);

            if (chat != null)
            {
                // 1. Удаляем все записи из ChatUsers, связанные с этим чатом
                var chatUsers = _dbContext.ChatUsers.Where(cu => cu.ChatId == chatId);
                _dbContext.ChatUsers.RemoveRange(chatUsers);

                // 2. Удаляем сам чат
                _dbContext.Chats.Remove(chat);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

}
