using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IChatRepository
    {
        Task<Chat?> GetById(int id);
        Task AddUserToChat(ChatUser chatUser);
        Task<bool> IsUserInChat(int chatId, int userId);
        Task<Chat> CreateChat(Chat chat);
        Task<List<ChatUser>> GetChatUsers(int chatId);
        Task<bool> ChatExists(int chatId);
    }
    public class ChatRepository : IChatRepository
    {
        private readonly JMgramDbContext _dbContext;

        public ChatRepository(JMgramDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Chat?> GetById(int id)
        {
            return await _dbContext.Chats.FindAsync(id);
        }

        public async Task AddUserToChat(ChatUser chatUser)
        {
            _dbContext.ChatUsers.Add(chatUser);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<List<ChatUser>> GetChatUsers(int chatId)
        {
            return await _dbContext.ChatUsers
                .Where(cu => cu.ChatId == chatId)
                .Include(cu => cu.User)
                .ToListAsync();
        }

        public async Task<bool> IsUserInChat(int chatId, int userId)
        {
            return await _dbContext.ChatUsers.AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
        }
        public async Task<Chat> CreateChat(Chat chat)
        {
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();
            return chat;
        }
        public async Task<bool> ChatExists(int chatId)
        {
            return await _dbContext.Chats.AnyAsync(c => c.Id == chatId);
        }
    }

}
