using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IContactRepository
    {
        Task<bool> IsContact(int userId, int contactUserId);
        Task Add(Contact contact);
        Task<List<Contact>> GetContactsForUser(int userId);
    }
    public class ContactRepository : IContactRepository
    {
        private readonly JMgramDbContext _dbContext;

        public ContactRepository(JMgramDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<bool> IsContact(int userId, int contactUserId)
        {
            return await _dbContext.Contacts.AnyAsync(c => c.UserId == userId && c.ContactUserId == contactUserId);
        }

        public async Task Add(Contact contact)
        {
            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Contact>> GetContactsForUser(int userId)
        {
            // Загружаем ContactUser
            return await _dbContext.Contacts
               .Include(c => c.ContactUser)  // Добавлена жадная загрузка
               .Where(c => c.UserId == userId)
               .ToListAsync();
        }
    }
}
