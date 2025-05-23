using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class AcceptContactRequestUseCase
    {
        private readonly IContactRequestRepository _contactRequestRepository;
        private readonly AddContactUseCase _addContactUseCase;
        private readonly CreatePrivateChatUseCase _createPrivateChatUseCase;
        private readonly ILogger<AcceptContactRequestUseCase> _logger;

        public AcceptContactRequestUseCase(
            IContactRequestRepository contactRequestRepository,
            CreatePrivateChatUseCase createPrivateChatUseCase, 
            ILogger<AcceptContactRequestUseCase> logger,
            AddContactUseCase addContactUseCase)
        {
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _createPrivateChatUseCase = createPrivateChatUseCase ?? throw new ArgumentNullException(nameof(createPrivateChatUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _addContactUseCase = addContactUseCase;
        }

        public async Task<AcceptContactRequestResponse> Execute(AcceptContactRequestRequest request)
        {
            _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Starting execution for ContactRequestId = {request.ContactRequestId}");

            try
            {
                // 1. Валидация входных данных
                if (request.ContactRequestId == Guid.Empty)
                {
                    _logger.LogError("AcceptContactRequestUseCase.Execute: Invalid input data - ContactRequestId is empty.");
                    return new AcceptContactRequestResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные: ContactRequestId не указан." };
                }

                // 2. Получить запрос в контакты
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Getting ContactRequest from repository for ContactRequestId = {request.ContactRequestId}");
                var contactRequest = await _contactRequestRepository.GetContactRequestById(request.ContactRequestId);
                if (contactRequest == null)
                {
                    _logger.LogError($"AcceptContactRequestUseCase.Execute: ContactRequest not found for ContactRequestId = {request.ContactRequestId}");
                    return new AcceptContactRequestResponse { IsSuccess = false, ErrorMessage = $"Запрос на добавление в друзья с ID {request.ContactRequestId} не найден." };
                }

                // Проверяем, что RecipientUserId соответствует текущему пользователю (можно получить из Claims)
                //  Предполагается, что у вас есть способ получить ID текущего пользователя
                //var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                //if (contactRequest.RecipientUserId != currentUserId)
                //{
                //    _logger.LogError($"AcceptContactRequestUseCase.Execute: User is not authorized to accept this ContactRequest.");
                //    return new AcceptContactRequestResponse { IsSuccess = false, ErrorMessage = "Вы не можете принять этот запрос на добавление в друзья." };
                //}

                // 3. Изменить статус запроса
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Updating ContactRequest status to Accepted.");
                contactRequest.Status = ContactRequestStatus.Accepted;
                await _contactRequestRepository.UpdateContactRequest(contactRequest);

                // 4. Создать приватный чат
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Creating private chat between SenderUserId = {contactRequest.SenderUserId} and RecipientUserId = {contactRequest.RecipientUserId}");
                var createPrivateChatRequest = new CreatePrivateChatRequest
                {
                    UserId1 = contactRequest.SenderUserId,
                    UserId2 = contactRequest.RecipientUserId
                };
                var createPrivateChatResponse = await _createPrivateChatUseCase.Execute(createPrivateChatRequest);
                if (!createPrivateChatResponse.IsSuccess)
                {
                    _logger.LogError($"AcceptContactRequestUseCase.Execute: Error creating private chat: {createPrivateChatResponse.ErrorMessage}");
                    return new AcceptContactRequestResponse { IsSuccess = false, ErrorMessage = $"Запрос на добавление в друзья принят, но произошла ошибка при создании чата: {createPrivateChatResponse.ErrorMessage}" };
                }
                var addContactRequest = new AddContactRequest()
                {
                    ContactUserId = contactRequest.SenderUserId,
                };
                var addContactResponse = await _addContactUseCase.Execute(addContactRequest);
                var addContactRequestForSender = new AddContactRequest()
                {
                    ContactUserId = contactRequest.RecipientUserId,
                };
                var addContactResponseForSender = await _addContactUseCase.Execute(addContactRequestForSender);

                // 5. Вернуть результат
                _logger.LogInformation("AcceptContactRequestUseCase.Execute: Successfully completed.");
                return new AcceptContactRequestResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"AcceptContactRequestUseCase.Execute: An error occurred: {ex.Message}");
                return new AcceptContactRequestResponse { IsSuccess = false, ErrorMessage = $"An error occurred while accepting contact request: {ex.Message}" };
            }
        }
    }
}