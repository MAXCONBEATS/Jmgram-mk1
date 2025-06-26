using Castle.Core.Logging;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IChatInvationRepository
    {
        Task AddChatInvitation(ChatInvitation chatInvitation);
        Task<ChatInvitation> GetChatInvitation(string chatId, string senderUserId, string recipientUserId);
        Task<ChatInvitation> GetChatInvitation(string chatId);
        Task<List<ChatInvitation>> GetChatInvitationsByChatId(string recipientUserId);
        Task<ChatInvitation> GetChatInvitationById(Guid chatInvitationId);
        Task<List<ChatInvitation>> GetIncomingChatInvitations(string userId);
        Task UpdateChatInvitation(ChatInvitation chatInvitation);
        Task DeleteChatInvitation(ChatInvitation chatInvitation);
        Task DeleteChatInvitationsByChatId(string chatId);
    }
    public class ChatInvationRepository : IChatInvationRepository
    {
        private readonly JMgramDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
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
        public async Task<ChatInvitation> GetChatInvitation(string chatId)
        {
            return await _dbContext.ChatInvitations.FirstOrDefaultAsync(ch => ch.ChatId == chatId);
        }
        public async Task<List<ChatInvitation>> GetChatInvitationsByChatId(string recipientUserId)
        {
            return await _dbContext.ChatInvitations.Where(ch => ch.RecipientUserId == recipientUserId).ToListAsync();
        }
        public async Task<ChatInvitation> GetChatInvitationById(Guid chatInvitationId)
        {
            return await _dbContext.ChatInvitations.FindAsync(chatInvitationId);
        }
        public async Task<List<ChatInvitation>> GetIncomingChatInvitations(string userId)
        {
            return await _dbContext.ChatInvitations.Where(ch => ch.RecipientUserId == userId).ToListAsync();
        }
        public async Task UpdateChatInvitation(ChatInvitation chatInvitation)
        {
            if (chatInvitation == null)
            {
                throw new ArgumentNullException(nameof(chatInvitation));
            }
            _dbContext.ChatInvitations.Update(chatInvitation);
            await _dbContext.SaveChangesAsync();

        }
        public async Task DeleteChatInvitation(ChatInvitation chatInvitation)
        {
            if (chatInvitation == null)
            {
                throw new ArgumentNullException(nameof(chatInvitation));
            }
            _dbContext.ChatInvitations.Remove(chatInvitation);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteChatInvitationsByChatId(string chatId)
        {
            await _dbContext.ChatInvitations
                .Where(ci => ci.ChatId == chatId)
                .ExecuteDeleteAsync();
        }
    }
}
