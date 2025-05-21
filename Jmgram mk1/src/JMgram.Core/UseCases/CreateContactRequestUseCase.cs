using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly SendNotificationUseCase _sendNotificationUseCase;
        private readonly ILogger<CreateContactRequestUseCase> _logger; // Add ILogger

        public CreateContactRequestUseCase(IContactRequestRepository contactRequestRepository, SendNotificationUseCase sendNotificationUseCase, ILogger<CreateContactRequestUseCase> logger)
        {
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Inject ILogger
        }

        public async Task<CreateContactRequestResponse> Execute(string senderUserId, string recipientUserId)
        {
            _logger.LogInformation("CreateContactRequestUseCase.Execute: Starting execution..."); // Add logging

            if (string.IsNullOrEmpty(recipientUserId))
            {
                _logger.LogWarning("CreateContactRequestUseCase.Execute: Invalid recipientUserId."); // Add logging
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = "Неверный идентификатор пользователя." };
            }

            if (senderUserId == recipientUserId)
            {
                _logger.LogWarning("CreateContactRequestUseCase.Execute: Cannot add self as contact."); // Add logging
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = "Нельзя добавить себя в друзья." };
            }

            // 1. Создать запрос на добавление в друзья
            var contactRequest = new ContactRequest
            {
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                Status = ContactRequestStatus.Pending
            };

            _logger.LogInformation("CreateContactRequestUseCase.Execute: ContactRequest created."); // Add logging

            try
            {
                // 2. Сохранить запрос в базу данных
                await _contactRequestRepository.AddContactRequest(contactRequest);
                _logger.LogInformation("CreateContactRequestUseCase.Execute: ContactRequest saved to database."); // Add logging

                // 3. Отправить уведомление
                // ... (your notification code)

                _logger.LogInformation("CreateContactRequestUseCase.Execute: Notification sent."); // Add logging

                _logger.LogInformation("CreateContactRequestUseCase.Execute: Execution completed successfully."); // Add logging
                return new CreateContactRequestResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateContactRequestUseCase.Execute: Error during execution: {ex.Message}. Inner exception: {ex.InnerException?.Message}"); // Add logging
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = $"Ошибка при создании запроса: {ex.Message}" };
            }
        }
    }
}
