using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
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
        private readonly IContactRepository _contactRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<AcceptContactRequestUseCase> _logger;
        private readonly CreateChatUseCase _createChatUseCase;

        public AcceptContactRequestUseCase(IContactRequestRepository contactRequestRepository, IContactRepository contactRepository, IChatRepository chatRepository,
            IUserRepository userRepository, ILogger<AcceptContactRequestUseCase> logger, CreateChatUseCase createChatUseCase) // Update constructor
        {
            _contactRequestRepository = contactRequestRepository ?? throw new ArgumentNullException(nameof(contactRequestRepository));
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createChatUseCase = createChatUseCase;
        }

        public async Task<bool> Execute(int contactRequestId)
        {
            try
            {
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Start accepting contact request with ID: {contactRequestId}");

                // 1. Get the ContactRequest by Id
                var contactRequest = await _contactRequestRepository.GetContactRequestById(contactRequestId);
                if (contactRequest == null)
                {
                    _logger.LogWarning($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} not found.");
                    return false; // or throw an exception
                }

                // 2. Check the status of the contact request
                if (contactRequest.Status != ContactRequestStatus.Pending)
                {
                    _logger.LogWarning($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} is not in Pending status. Current status: {contactRequest.Status}");
                    return false; // or throw an exception, or handle it differently
                }

                // 3. Update the ContactRequest status to Accepted
                await _contactRequestRepository.UpdateContactRequestStatus(contactRequestId, ContactRequestStatus.Accepted);
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} status updated to Accepted.");

                // 4. Create contact entries for both users
                // 4a. Create contact entry for the request sender
                var senderProfile = await _userRepository.GetUserProfileById(contactRequest.SenderUserId); // Get sender profile
                var recipientProfile = await _userRepository.GetUserProfileById(contactRequest.RecipientUserId); // Get recipient profile для имени
                var senderContact = new Contact
                {
                    UserId = contactRequest.SenderUserId,
                    ContactUserId = contactRequest.RecipientUserId,
                    Name = recipientProfile?.FirstName ?? "Неизвестный", //  Получаем имя из профиля контакта
                    Phone = senderProfile?.Phone // Use phone from sender profile
                };
                await _contactRepository.Add(senderContact);
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact created for sender: {contactRequest.SenderUserId}, ContactUserId: {contactRequest.RecipientUserId}");

                // 4b. Create contact entry for the request recipient
                var recipientProfile2 = await _userRepository.GetUserProfileById(contactRequest.RecipientUserId); // Get recipient profile
                var senderProfile2 = await _userRepository.GetUserProfileById(contactRequest.SenderUserId); // Get sender profile для имени
                var recipientContact = new Contact
                {
                    UserId = contactRequest.RecipientUserId,
                    ContactUserId = contactRequest.SenderUserId,
                    Name = senderProfile2?.FirstName ?? "Неизвестный", //  Получаем имя из профиля контакта
                    Phone = recipientProfile2?.Phone // Use phone from recipient profile
                };
                await _contactRepository.Add(recipientContact);
                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact created for recipient: {contactRequest.RecipientUserId}, ContactUserId: {contactRequest.SenderUserId}");

                // 5. Create or get chat
                // Get chat if it exists
                var existingChat = await _chatRepository.GetChatBetweenUsers(contactRequest.SenderUserId, contactRequest.RecipientUserId);

                if (existingChat == null)
                {
                    // Chat does not exist, create it
                    _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Chat does not exist between users, creating new chat");

                    // Get user profiles to get names for the chat title
                    var senderFirstName = senderProfile?.FirstName ?? "Неизвестный";
                    var recipientFirstName = recipientProfile?.FirstName ?? "Неизвестный";
                    var chatName = $"Переписка с {recipientFirstName}";

                    var chatDto = new ChatDto
                    {
                        Name = chatName
                    };

                    var createChatRequest = new CreateChatRequest
                    {
                        Chat = chatDto,
                        Phones = new List<string> { senderProfile?.Phone, recipientProfile?.Phone } // Используем номера телефонов
                    };

                    var chatResponse = await _createChatUseCase.Execute(createChatRequest);

                    if (!chatResponse.IsSuccess)
                    {
                        _logger.LogError($"AcceptContactRequestUseCase.Execute: Failed to create chat for users {contactRequest.SenderUserId} and {contactRequest.RecipientUserId}: {chatResponse.ErrorMessage}");
                        // Handle the error, maybe log it, but don't necessarily fail the whole operation
                    }
                    else
                    {
                        _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Successfully created new chat between users");
                    }
                }
                else
                {
                    // Chat exists, add the current user to it
                    _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Chat already exists between users, adding user to chat");

                    var recipientChatUser = new ChatUser
                    {
                        ChatId = existingChat.Id,
                        UserId = contactRequest.RecipientUserId,
                        JoinedAt = DateTime.UtcNow
                    };

                    await _chatRepository.AddUserToChat(recipientChatUser);
                    _logger.LogInformation($"AcceptContactRequestUseCase.Execute: User {contactRequest.RecipientUserId} added to existing chat {existingChat.Id}");
                }

                _logger.LogInformation($"AcceptContactRequestUseCase.Execute: Contact request with ID {contactRequestId} accepted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AcceptContactRequestUseCase.Execute: An error occurred while accepting contact request with ID {contactRequestId}: {ex.Message}");
                return false;
            }
        }
    }
}