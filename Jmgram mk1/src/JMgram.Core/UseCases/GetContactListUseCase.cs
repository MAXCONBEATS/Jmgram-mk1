
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetContactListUseCase
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILogger<GetContactListUseCase> _logger;

        public GetContactListUseCase(IContactRepository contactRepository, ILogger<GetContactListUseCase> logger)
        {
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ContactListResponse> Execute(string userId)
        {
            try
            {
                _logger.LogInformation($"GetContactListUseCase.Execute: Getting contact list for UserId: {userId}");

                var contacts = await _contactRepository.GetContactList(userId);

                var contactDtos = contacts.Select(c => new ContactDto
                {
                    UserId = c.UserId,
                    ContactUserId = c.ContactUserId,
                    Name = c.Name,
                    Phone = c.Phone
                }).ToList();

                _logger.LogInformation($"GetContactListUseCase.Execute: Successfully retrieved contact list for UserId: {userId}");

                return new ContactListResponse
                {
                    IsSuccess = true,
                    Contacts = contactDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetContactListUseCase.Execute: An error occurred while getting contact list for UserId: {userId}: {ex.Message}");
                return new ContactListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Не удалось получить список контактов."
                };
            }
        }
    }
}
