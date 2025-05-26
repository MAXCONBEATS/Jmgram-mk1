using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class CreateContactRequestUseCase
    {
        private readonly IContactRequestRepository _contactRequestRepository;
        private readonly SendNotificationUseCase _sendNotificationUseCase;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CreateContactRequestUseCase> _logger;

        public CreateContactRequestUseCase(
            IContactRequestRepository contactRequestRepository,
            SendNotificationUseCase sendNotificationUseCase,
            IUserRepository userRepository,
            ILogger<CreateContactRequestUseCase> logger)
        {
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CreateContactRequestResponse> Execute(string senderUserId, string recipientUserId)
        {
            _logger.LogInformation("CreateContactRequestUseCase.Execute: Starting execution...");

            if (string.IsNullOrEmpty(recipientUserId))
            {
                _logger.LogWarning("CreateContactRequestUseCase.Execute: Invalid recipientUserId.");
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = "Неверный идентификатор пользователя." };
            }

            if (senderUserId == recipientUserId)
            {
                _logger.LogWarning("CreateContactRequestUseCase.Execute: Cannot add self as contact.");
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = "Нельзя добавить себя в друзья." };
            }

            var contactRequest = new ContactRequest
            {
                Id = Guid.NewGuid(),
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                Status = ContactRequestStatus.Pending
            };

            var existingContactRequest = await _contactRequestRepository.GetContactRequest(senderUserId, recipientUserId);

            if (existingContactRequest != null)
            {
                _logger.LogWarning($"CreateContactRequestUseCase.Execute: Contact request already exists between {senderUserId} and {recipientUserId}.");
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = "Запрос на добавление в друзья уже существует." };
            }

            _logger.LogInformation("CreateContactRequestUseCase.Execute: ContactRequest created.");

            try
            {
                await _contactRequestRepository.AddContactRequest(contactRequest);
                _logger.LogInformation("CreateContactRequestUseCase.Execute: ContactRequest saved to database.");

                var senderUser = await _userRepository.GetById(senderUserId); 
                string senderName = senderUser?.FirstName ?? "Неизвестный пользователь"; 
                NotificationDto notificationDto = new NotificationDto
                {
                    UserId = recipientUserId,
                    NotificationType = NotificationType.ContactRequest,
                    Message = $"Запрос на добавление в друзья от {senderName}",
                    Timestamp = DateTime.UtcNow,
                    IsRead = false,
                    ChatId = null
                };

                var sendNotificationResponse = await _sendNotificationUseCase.Execute(notificationDto, senderUserId);

                if (!sendNotificationResponse.IsSuccess)
                {
                    _logger.LogError($"CreateContactRequestUseCase.Execute: Failed to send notification: {sendNotificationResponse.ErrorMessage}");
                    return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = $"Ошибка при отправке уведомления: {sendNotificationResponse.ErrorMessage}" };
                }

                _logger.LogInformation("CreateContactRequestUseCase.Execute: Notification sent.");

                _logger.LogInformation("CreateContactRequestUseCase.Execute: Execution completed successfully.");
                return new CreateContactRequestResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateContactRequestUseCase.Execute: Error during execution: {ex.Message}. Inner exception: {ex.InnerException?.Message}");
                return new CreateContactRequestResponse { IsSuccess = false, ErrorMessage = $"Ошибка при создании запроса: {ex.Message}" };
            }
        }
    }
}
