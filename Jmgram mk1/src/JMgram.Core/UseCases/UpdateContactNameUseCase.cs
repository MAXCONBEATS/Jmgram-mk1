using Jmgram_mk1.src.JMgram.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class UpdateContactNameUseCase
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILogger<UpdateContactNameUseCase> _logger;

        public UpdateContactNameUseCase(IContactRepository contactRepository, ILogger<UpdateContactNameUseCase> logger)
        {
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Execute(string userId, string contactUserId, string newName)
        {
            try
            {
                _logger.LogInformation($"UpdateContactNameUseCase.Execute: Updating contact name for UserId: {userId}, ContactUserId: {contactUserId} to Name: {newName}");

                var contact = await _contactRepository.GetContact(userId, contactUserId);
                if (contact == null)
                {
                    _logger.LogWarning($"UpdateContactNameUseCase.Execute: Contact not found for UserId: {userId}, ContactUserId: {contactUserId}");
                    return false;
                }

                contact.Name = newName;
                await _contactRepository.Update(contact); // Assuming you have an Update method in your repository

                _logger.LogInformation($"UpdateContactNameUseCase.Execute: Contact name updated successfully for UserId: {userId}, ContactUserId: {contactUserId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateContactNameUseCase.Execute: An error occurred while updating contact name for UserId: {userId}, ContactUserId: {contactUserId}: {ex.Message}");
                return false;
            }
        }
    }
}
