using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.EntityFrameworkCore;

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
    }
    public class ChatRepository : IChatRepository
    {
        private readonly JMgramDbContext _dbContext;

        public ChatRepository(JMgramDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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

        public async Task<Chat> CreateChat(Chat chat)
        {
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();
            return chat;
        }

        public async Task<bool> ChatExists(string chatId) // Изменено на string
        {
            return await _dbContext.Chats.AnyAsync(c => c.Id == chatId);
        }
        public async Task<bool> IsUserInChat(string chatId, string userId)
        {
            return await _dbContext.ChatUsers.AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
        }
    }

}
