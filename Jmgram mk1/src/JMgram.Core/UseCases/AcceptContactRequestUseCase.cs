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
    public class AcceptContactRequestUseCase
    {
        private readonly IContactRequestRepository _contactRequestRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IUserRepository _userRepository; // Add this
        private readonly ILogger<AcceptContactRequestUseCase> _logger;

        public AcceptContactRequestUseCase(IContactRequestRepository contactRequestRepository, IContactRepository contactRepository, IUserRepository userRepository, ILogger<AcceptContactRequestUseCase> logger) // Update constructor
        {
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository)); // Initialize
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Execute(int contactRequestId)
        {
            try
            {
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Start accepting contact request with ID: {contactRequestId}");

                // 1. Get the ContactRequest by Id
                var contactRequest = await _contactRequestRepository.GetContactRequestById(contactRequestId);
                if (contactRequest == null)
                {
                    _logger.LogWarning($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} not found.");
                    return false; // or throw an exception
                }

                // 2. Check the status of the contact request
                if (contactRequest.Status != ContactRequestStatus.Pending)
                {
                    _logger.LogWarning($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} is not in Pending status. Current status: {contactRequest.Status}");
                    return false; // or throw an exception, or handle it differently
                }

                // 3. Update the ContactRequest status to Accepted
                await _contactRequestRepository.UpdateContactRequestStatus(contactRequestId, ContactRequestStatus.Accepted);
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} status updated to Accepted.");

                // 4. Create contact entries for both users
                // 4a. Create contact entry for the request sender
                var senderProfile = await _userRepository.GetUserProfileById(contactRequest.SenderUserId); // Get sender profile
                var senderContact = new Contact
                {
                    UserId = contactRequest.SenderUserId,
                    ContactUserId = contactRequest.RecipientUserId,
                    Name = "TODO", // You may retrieve name from user profile
                    Phone = senderProfile?.Phone // Use phone from sender profile
                };
                await _contactRepository.Add(senderContact);
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact created for sender: {contactRequest.SenderUserId}, ContactUserId: {contactRequest.RecipientUserId}");

                // 4b. Create contact entry for the request recipient
                var recipientProfile = await _userRepository.GetUserProfileById(contactRequest.RecipientUserId); // Get recipient profile
                var recipientContact = new Contact
                {
                    UserId = contactRequest.RecipientUserId,
                    ContactUserId = contactRequest.SenderUserId,
                    Name = "TODO", // You may retrieve name from user profile
                    Phone = recipientProfile?.Phone // Use phone from recipient profile
                };
                await _contactRepository.Add(recipientContact);
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact created for recipient: {contactRequest.RecipientUserId}, ContactUserId: {contactRequest.SenderUserId}");

                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} accepted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AcceptContactRequestUseCase.Execute: An error occurred while accepting contact request with ID {contactRequestId}: {ex.Message}");
                return false;
            }
        }
    }
}