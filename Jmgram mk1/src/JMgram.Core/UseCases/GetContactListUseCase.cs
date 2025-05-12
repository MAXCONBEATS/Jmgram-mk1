
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetContactListUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IContactRepository _contactRepository;

        public GetContactListUseCase(IUserRepository userRepository, IContactRepository contactRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        }

        public async Task<GetContactListResponse> Execute(GetContactListRequest request)
        {
            // 1. Проверить входные данные
            if (request.UserId <= 0)
            {
                return new GetContactListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "UserId must be greater than 0.",
                    Contacts = new List<ContactDto>()
                };
            }

            // 2. Получить пользователя (для проверки существования)
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
            {
                return new GetContactListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"User with UserId {request.UserId} not found.",
                    Contacts = new List<ContactDto>()
                };
            }

            // 3. Получить список контактов для пользователя
            List<Contact> contacts;
            try
            {
                contacts = await _contactRepository.GetContactsForUser(request.UserId);
            }
            catch (Exception ex)
            {
                return new GetContactListResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error retrieving contacts: {ex.Message}",
                    Contacts = new List<ContactDto>()
                };
            }

            // 4. Преобразовать контакты в DTO
            var contactDtos = contacts.Select(c => new ContactDto
            {
                Id = c.Id,
                UserId = c.UserId,
                ContactUserId = c.ContactUserId,
                AddedAt = c.AddedAt
            }).ToList();

            // 5. Вернуть результат
            return new GetContactListResponse
            {
                IsSuccess = true,
                Contacts = contactDtos
            };
        }
    }
}
