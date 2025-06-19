using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetChatInvitationsResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<ChatInvitation> IncomingRequests { get; set; }
    }
    public class GetChatInvitationsUseCase
    {
        private readonly IChatInvationRepository _chatInvitationRepository;
        private readonly ILogger<GetContactRequestsUseCase> _logger;
        public GetChatInvitationsUseCase(IChatInvationRepository chatInvitationRepository, ILogger<GetContactRequestsUseCase> logger)
        {
            _chatInvitationRepository = chatInvitationRepository;
            _logger = logger;
        }
        public async Task<GetChatInvitationsResponse> Execute(string userId)
        {
            try
            {
                var incomingChatInvitations = await _chatInvitationRepository.GetIncomingChatInvitations(userId);
                return new GetChatInvitationsResponse
                {
                    IsSuccess = true,
                    IncomingRequests = incomingChatInvitations
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"GetChatInvitationsUseCase.Execute: An error occurred while retrieving contact requests for user {userId}: {e.Message}, Inner Exception: {e.InnerException}");
                return new GetChatInvitationsResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Ошибка при получении запросов: {e.Message}",
                    IncomingRequests = null
                };
            }
        }
    }
}
