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
        private readonly ILogger<DeleteContactUseCase> _logger;

        public DeleteContactUseCase(IContactRepository contactRepository, ILogger<DeleteContactUseCase> logger)
        {
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Execute(string userId, string contactUserId)
        {
            try
            {
                _logger.LogInformation($"DeleteContactUseCase.Execute: Deleting contact for UserId: {userId}, ContactUserId: {contactUserId}");

                // Delete contact for the current user
                await _contactRepository.Delete(userId, contactUserId);
                _logger.LogInformation($"DeleteContactUseCase.Execute: Contact deleted successfully for UserId: {userId}, ContactUserId: {contactUserId}");

                // Delete contact for the other user
                await _contactRepository.Delete(contactUserId, userId); // Delete the reverse relation
                _logger.LogInformation($"DeleteContactUseCase.Execute: Contact deleted successfully for UserId: {contactUserId}, ContactUserId: {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteContactUseCase.Execute: An error occurred while deleting contact for UserId: {userId}, ContactUserId: {contactUserId}: {ex.Message}");
                return false;
            }
        }
    }
}
