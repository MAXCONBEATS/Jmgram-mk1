using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IChatInvationRepository
    {
        Task AddChatInvitation(ChatInvitation chatInvitation);
        Task<ChatInvitation> GetChatInvitation(string chatId, string senderUserId, string recipientUserId);
        Task<ChatInvitation> GetChatInvitationById(Guid chatInvitationId);
    }
    public class ChatInvationRepository : IChatInvationRepository
    {
        private readonly JMgramDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ChatInvationRepository(JMgramDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task AddChatInvitation(ChatInvitation chatInvitation)
        {
            _dbContext.ChatInvitations.Add(chatInvitation);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<ChatInvitation> GetChatInvitation(string chatId, string senderUserId, string recipientUserId)
        {
            return await _dbContext.ChatInvitations.FirstOrDefaultAsync(ch => ch.ChatId == chatId && ch.SenderUserId == senderUserId && ch.RecipientUserId == recipientUserId);
        }
        public async Task<ChatInvitation> GetChatInvitationById(Guid chatInvitationId)
        {
            return await _dbContext.ChatInvitations.FindAsync(chatInvitationId);
        }
    }
}
