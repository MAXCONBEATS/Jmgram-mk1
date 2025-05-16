using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IContactRepository
    {
        Task<bool> IsContact(string userId, string contactUserId);
        Task Add(Contact contact);
        Task<Contact> GetContact(string userId, string contactUserId);
        Task<List<Contact>> GetContactsForUser(string userId);
        Task Update(Contact contact);
        Task Delete(string userId, string contactUserId);
    }
    public class ContactRepository : IContactRepository
    {
        private readonly JMgramDbContext _dbContext;

        public ContactRepository(JMgramDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<bool> IsContact(string userId, string contactUserId)
        {
            return await _dbContext.Contacts.AnyAsync(c => c.UserId == userId && c.ContactUserId == contactUserId);
        }

        public async Task Add(Contact contact)
        {
            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Contact> GetContact(string userId, string contactUserId)
        {
            return await _dbContext.Contacts.FirstOrDefaultAsync(c => c.UserId == userId && c.ContactUserId == contactUserId);
        }

        public async Task<List<Contact>> GetContactsForUser(string userId)
        {
            return await _dbContext.Contacts
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
        public async Task Update(Contact contact)
        {
            _dbContext.Contacts.Update(contact);
            await _dbContext.SaveChangesAsync();
        }
        public async Task Delete(string userId, string contactUserId)
        {
            var contact = await _dbContext.Contacts.FirstOrDefaultAsync(c => c.UserId == userId && c.ContactUserId == contactUserId);
            if (contact != null)
            {
                _dbContext.Contacts.Remove(contact);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
