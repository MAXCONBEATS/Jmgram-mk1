using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public interface IGetContactRequestsUseCase
    {
        Task<GetContactRequestsResponse> Execute(string userId);
    }


    public class GetContactRequestsResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<ContactRequest> IncomingRequests { get; set; }
        public List<ContactRequest> OutgoingRequests { get; set; }
    }
    public class GetContactRequestsUseCase : IGetContactRequestsUseCase
    {
        private readonly IContactRequestRepository _contactRequestRepository;
        private readonly ILogger<GetContactRequestsUseCase> _logger;

        public GetContactRequestsUseCase(IContactRequestRepository contactRequestRepository, ILogger<GetContactRequestsUseCase> logger)
        {
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetContactRequestsResponse> Execute(string userId)
        {
            _logger.LogInformation($"GetContactRequestsUseCase.Execute: Attempting to retrieve contact requests for user {userId}.");

            try
            {
                var incomingRequests = await _contactRequestRepository.GetIncomingContactRequests(userId);
                var outgoingRequests = await _contactRequestRepository.GetOutgoingContactRequests(userId);

                _logger.LogInformation($"GetContactRequestsUseCase.Execute: Retrieved incoming requests: {incomingRequests.Count}, outgoing requests: {outgoingRequests.Count} for user {userId}.");

                return new GetContactRequestsResponse
                {
                    IsSuccess = true,
                    IncomingRequests = incomingRequests,
                    OutgoingRequests = outgoingRequests
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetContactRequestsUseCase.Execute: An error occurred while retrieving contact requests for user {userId}: {ex.Message}, Inner Exception: {ex.InnerException}");
                return new GetContactRequestsResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Ошибка при получении запросов: {ex.Message}",
                    IncomingRequests = null,
                    OutgoingRequests = null
                };
            }
        }
    }
    
}
