
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class AddContactUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IContactRepository _contactRepository; // Предполагаем, что есть репозиторий для контактов

        public AddContactUseCase(IUserRepository userRepository, IContactRepository contactRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        }

        public async Task<AddContactResponse> Execute(AddContactRequest request)
        {
            // 1.  Проверить входные данные
            if (request.UserId <= 0 || request.ContactUserId <= 0)
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Неверные идентификаторы пользователей." };
            }

            if (request.UserId == request.ContactUserId)
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Нельзя добавить себя в контакты." };
            }

            // 2.  Проверить, что оба пользователя существуют
            var user = await _userRepository.GetById(request.UserId);
            var contactUser = await _userRepository.GetById(request.ContactUserId);

            if (user == null || contactUser == null)
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Один или оба пользователя не найдены." };
            }

            // 3. Проверить, что контакт еще не добавлен
            if (await _contactRepository.IsContact(request.UserId, request.ContactUserId))
            {
                return new AddContactResponse { IsSuccess = false, ErrorMessage = "Этот пользователь уже добавлен в контакты." };
            }

            // 4. Создать новый контакт
            var contact = new Contact
            {
                UserId = request.UserId,
                ContactUserId = request.ContactUserId,
                AddedAt = DateTime.UtcNow
            };

            // 5.  Сохранить контакт в базе данных
            await _contactRepository.Add(contact);

            // 6. Преобразовать в DTO
            var contactDto = new ContactDto
            {
                Id = contact.Id,
                UserId = contact.UserId,
                ContactUserId = contact.ContactUserId,
                AddedAt = contact.AddedAt
            };

            // 7.  Вернуть результат
            return new AddContactResponse { IsSuccess = true, Contact = contactDto };
        }
    }
}
