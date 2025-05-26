using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.EntityFrameworkCore;
namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetMessagesForChat(string chatId, int pageNumber, int pageSize);
        Task<int> GetTotalMessageCount(string chatId);
        Task<int> Add(Message message);
        Task<Message?> GetMessageById(int messageId); 
        Task Update(Message message);
    }
    public class MessageRepository : IMessageRepository
    {
        private readonly JMgramDbContext _dbContext;

        public MessageRepository(JMgramDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<Message>> GetMessagesForChat(string chatId, int pageNumber, int pageSize)
        {
            return await _dbContext.Messages
             .Where(m => m.ChatId == chatId)
             .OrderByDescending(m => m.Timestamp)
             .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync();
        }

        public async Task<int> GetTotalMessageCount(string chatId)
        {
            return await _dbContext.Messages.CountAsync(m => m.ChatId == chatId);
        }
        public async Task<int> Add(Message message)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            return message.Id;
        }

        public async Task<Message?> GetMessageById(int messageId)
        {
            return await _dbContext.Messages.FindAsync(messageId);
        }

        public async Task Update(Message message)
        {
            _dbContext.Messages.Update(message);
            await _dbContext.SaveChangesAsync();
        }
    }

}
