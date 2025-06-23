using Azure.Core;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class UpdateMessageTextUseCase
    {
        private readonly IChatRepository _chatRepository;
        private readonly JMgramDbContext _context;
        private readonly ILogger<UpdateMessageTextUseCase> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UpdateMessageTextUseCase(IChatRepository chatRepository, ILogger<UpdateMessageTextUseCase> logger, IHttpContextAccessor httpContextAccessor, JMgramDbContext context)
        {
            _chatRepository = chatRepository;
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

        }
        public async Task<UpdateMessageTextResponse> Execute(UpdateMessageTextRequest updateMessageTextRequest)
        {
            try
            {
                var message = await _chatRepository.GetMessageById(updateMessageTextRequest.MessageId);
                if (message == null)
                {
                    _logger.LogWarning($"Message not found.");
                    return new UpdateMessageTextResponse { IsSuccess = false, ErrorMessage = "Message not found" };
                }
                if (updateMessageTextRequest.MessageId != null)
                {
                    message.Text = updateMessageTextRequest.Text;
                }
                await _context.SaveChangesAsync();
                return new UpdateMessageTextResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message text");
                return new UpdateMessageTextResponse { IsSuccess = false, ErrorMessage = "Internal server error" };
            }
        }
    }
}
