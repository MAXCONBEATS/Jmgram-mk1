using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IContactRequestRepository
    {
        Task AddContactRequest(ContactRequest contactRequest);
        Task UpdateContactRequestStatus(int contactRequestId, ContactRequestStatus status);
        Task<ContactRequest> GetContactRequestById(Guid contactRequestId);
        Task<ContactRequest> GetContactRequest(string senderUserId, string recipientUserId);
        Task UpdateContactRequest(ContactRequest contactRequest);
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

        public async Task UpdateContactRequestStatus(int contactRequestId, ContactRequestStatus status)
        {
            var contactRequest = await _context.ContactRequests.FindAsync(contactRequestId);
            if (contactRequest != null)
            {
                contactRequest.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ContactRequest> GetContactRequestById(Guid contactRequestId)
        {
            return await _context.ContactRequests.FindAsync(contactRequestId);
        }
        public async Task<ContactRequest> GetContactRequest(string senderUserId, string recipientUserId)
        {
            return await _context.ContactRequests
                .FirstOrDefaultAsync(cr => cr.SenderUserId == senderUserId && cr.RecipientUserId == recipientUserId);
        }
        public async Task UpdateContactRequest(ContactRequest contactRequest)
        {
            _context.ContactRequests.Update(contactRequest);
            await _context.SaveChangesAsync();
        }
    }
}

