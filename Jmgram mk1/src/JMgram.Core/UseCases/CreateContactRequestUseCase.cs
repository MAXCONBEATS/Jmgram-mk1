using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class CreateContactRequestUseCase
    {
        private readonly IContactRequestRepository _contactRequestRepository;
        private readonly IServiceProvider _serviceProvider;

        public CreateContactRequestUseCase(IContactRequestRepository contactRequestRepository, IServiceProvider serviceProvider)
        {
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<CreateContactRequestResponse> Execute(string senderUserId, string recipientUserId)
        {
            if (string.IsNullOrEmpty(recipientUserId))
            {
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = "Неверный идентификатор пользователя." };
            }

            if (senderUserId == recipientUserId)
            {
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = "Нельзя добавить себя в друзья." };
            }

            // 1. Создать запрос на добавление в друзья
            var contactRequest = new ContactRequest
            {
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                Status = ContactRequestStatus.Pending
            };

            try
            {
                // 2. Сохранить запрос в базу данных
                await _contactRequestRepository.AddContactRequest(contactRequest);

                // 3. Отправить уведомление (используем IServiceProvider)
                var sendNotificationUseCase = _serviceProvider.GetRequiredService<SendNotificationUseCase>();
                var notificationDto = new ContactRequestNotificationDto
                {
                    UserId = recipientUserId,
                    Message = "Новый запрос в контакты",
                    NotificationType = NotificationType.ContactRequest,
                    SenderUserId = senderUserId
                };
                var sendNotificationResponse = await sendNotificationUseCase.Execute(notificationDto);

                // 4. Вернуть успешный результат
                return new CreateContactRequestResponse { IsSuccess = sendNotificationResponse.IsSuccess, ErrorMessage = sendNotificationResponse.ErrorMessage };
            }
            catch (Exception ex)
            {
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = $"Ошибка при создании запроса: {ex.Message}" };
            }
        }
    }
}
