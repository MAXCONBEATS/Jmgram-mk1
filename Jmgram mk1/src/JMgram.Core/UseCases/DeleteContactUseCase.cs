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
    public class DeleteContactUseCase
    {
        private readonly IContactRepository _contactRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IContactRequestRepository _contactRequestRepository;
        private readonly ILogger<DeleteContactUseCase> _logger;

        public DeleteContactUseCase(IContactRepository contactRepository, IChatRepository chatRepository, ILogger<DeleteContactUseCase> logger, IContactRequestRepository contactRequestRepository)
        {
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        }

        public async Task<bool> Execute(string userId, string contactUserId)
        {
            try
            {

                await _contactRepository.Delete(userId, contactUserId);
                await _contactRepository.Delete(contactUserId, userId);

                var chat = await _chatRepository.GetChatBetweenUsers(userId, contactUserId);

                if (chat != null)
                {
                    await _chatRepository.DeleteChat(chat.Id);
                }
                else
                {
                    _logger.LogInformation($"DeleteContactUseCase.Execute: No chat found between users {userId} and {contactUserId}");
                }

                var contactRequest1 = await _contactRequestRepository.GetContactRequest(userId, contactUserId);
                var contactRequest2 = await _contactRequestRepository.GetContactRequest(contactUserId, userId);
                if (contactRequest1 != null)
                {
                    await _contactRequestRepository.DeleteContactRequest(contactRequest1);
                    _logger.LogInformation($"DeleteContactUseCase.Execute: ContactRequest {contactRequest1.Id} deleted successfully.");
                }
                else
                {
                    _logger.LogInformation($"DeleteContactUseCase.Execute: No ContactRequest found (UserId: {userId}, ContactUserId: {contactUserId})");
                }
                if (contactRequest2 != null)
                {
                    await _contactRequestRepository.DeleteContactRequest(contactRequest2);
                    _logger.LogInformation($"DeleteContactUseCase.Execute: ContactRequest {contactRequest2.Id} deleted successfully.");
                }
                else
                {
                    _logger.LogInformation($"DeleteContactUseCase.Execute: No ContactRequest found (UserId: {userId}, ContactUserId: {contactUserId})");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteContactUseCase.Execute: An error occurred while deleting contact/chat for UserId: {userId}, ContactUserId: {contactUserId}: {ex.Message}");
                return false;
            }
        }
    }
}
