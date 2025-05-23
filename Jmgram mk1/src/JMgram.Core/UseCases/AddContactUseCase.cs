
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class AddContactUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AddContactUseCase(IUserRepository userRepository, IContactRepository contactRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<AddContactResponse> Execute(AddContactRequest request)
        {
            // 1. Get user ID from the context
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "User ID not found in claims." };
            }

            // 2. Validate input
            if (string.IsNullOrEmpty(request.ContactUserId))
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Неверные идентификаторы пользователей." };
            }

            if (userId == request.ContactUserId)
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Нельзя добавить себя в контакты." };
            }

            // 3. Check if both users exist
            var user = await _userRepository.GetById(userId);
            var contactUser = await _userRepository.GetById(request.ContactUserId);

            if (user == null || contactUser == null)
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Один или оба пользователя не найдены." };
            }

            // 4. Check if the contact is already added
            if (await _contactRepository.IsContact(userId, request.ContactUserId))
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Этот пользователь уже добавлен в контакты." };
            }

            // 5. Create the contact
            var contact = new Contact
            {
                UserId = userId,
                ContactUserId = request.ContactUserId,
                Name = contactUser.FirstName,
                Phone = contactUser.Phone
            };
            var contactForRecipient = new Contact
            {
                UserId = request.ContactUserId,
                ContactUserId = userId,
                Name = user.FirstName,
                Phone = user.Phone
            };

            // 6. Save the contact in the database
            await _contactRepository.Add(contact);
            await _contactRepository.Add(contactForRecipient);

            // 7. Convert to DTO
            var contactDto = new ContactDto
            {
                UserId = contact.UserId,
                ContactUserId = contact.ContactUserId,
                Name = user.FirstName
            };
            // 8. Return result
            return new AddContactResponse { IsSuccess = true, Contact = contactDto };
        }
    }
}
