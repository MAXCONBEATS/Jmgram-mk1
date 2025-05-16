using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IContactRequestRepository
    {
        Task AddContactRequest(ContactRequest contactRequest);
    }
    public class ContactRequestRepository : IContactRequestRepository
    {
        private readonly JMgramDbContext _context;

        public ContactRequestRepository(JMgramDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddContactRequest(ContactRequest contactRequest)
        {
            _context.ContactRequests.Add(contactRequest);
            await _context.SaveChangesAsync();
        }
    }

}
