using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Jmgram_mk1.src.JMgram.Core.Entities;
using System.ComponentModel.DataAnnotations;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Services;


[Authorize]
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;

    private readonly CreateChatUseCase _createChatUseCase;
    private readonly AddUserToChatUseCase _addUserToChatUseCase;
    private readonly IGetChatListUseCase _getChatListUseCase;
    private readonly ISendMessageUseCase _sendMessageUseCase;
    private readonly SendNotificationUseCase _sendNotificationUseCase;
    private readonly RespondToChatInviteUseCase _respondToChatInviteUseCase;
    private readonly UpdateMessageStatusUseCase _updateMessageStatusUseCase;
    private readonly GetChatMessagesUseCase _getChatMessagesUseCase;
    private readonly GetUserChatsUseCase _getUserChatsUseCase;
    private readonly IGetLastChatMessageUseCase _getLastChatMessageUseCase; //  Измените тип здесь!
    private readonly RemoveUserFromChatUseCase _removeUserFromChatUseCase;
    private readonly DeleteChatUseCase _deleteChatUseCase;
    private readonly GetContactListUseCase _getContactListUseCase;
    private readonly IChatService _chatService;


    public ChatController(ILogger<ChatController> logger, CreateChatUseCase createChatUseCase, AddUserToChatUseCase addUserToChatUseCase,
     IHttpContextAccessor httpContextAccessor, IGetChatListUseCase getChatListUseCase,
     ISendMessageUseCase sendMessageUseCase, SendNotificationUseCase sendNotificationUseCase, IGetLastChatMessageUseCase getLastChatMessageUseCase,
     UpdateMessageStatusUseCase updateMessageStatusUseCase, GetChatMessagesUseCase getChatMessagesUseCase, GetUserChatsUseCase getUserChatsUseCase,
     GetContactListUseCase getContactListUseCase, IChatService chatService,
     RemoveUserFromChatUseCase removeUserFromChatUseCase, DeleteChatUseCase deleteChatUseCase,
     RespondToChatInviteUseCase respondToChatInviteUseCase, IChatRepository chatRepository, IUserRepository userRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _createChatUseCase = createChatUseCase ?? throw new ArgumentNullException(nameof(createChatUseCase));
        _addUserToChatUseCase = addUserToChatUseCase ?? throw new ArgumentNullException(nameof(addUserToChatUseCase));
        _getChatListUseCase = getChatListUseCase ?? throw new ArgumentNullException(nameof(getChatListUseCase));
        _sendMessageUseCase = sendMessageUseCase ?? throw new ArgumentNullException(nameof(sendMessageUseCase));
        _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
        _respondToChatInviteUseCase = respondToChatInviteUseCase ?? throw new ArgumentNullException(nameof(respondToChatInviteUseCase));
        _updateMessageStatusUseCase = updateMessageStatusUseCase ?? throw new ArgumentNullException(nameof(updateMessageStatusUseCase));
        _getChatMessagesUseCase = getChatMessagesUseCase ?? throw new ArgumentNullException(nameof(getChatMessagesUseCase));
        _getUserChatsUseCase = getUserChatsUseCase ?? throw new ArgumentNullException(nameof(getUserChatsUseCase));
        _getContactListUseCase = getContactListUseCase ?? throw new ArgumentNullException(nameof(getContactListUseCase));
        _getLastChatMessageUseCase = getLastChatMessageUseCase ?? throw new ArgumentNullException(nameof(getLastChatMessageUseCase));
        _removeUserFromChatUseCase = removeUserFromChatUseCase ?? throw new ArgumentNullException(nameof(removeUserFromChatUseCase));
        _deleteChatUseCase = deleteChatUseCase ?? throw new ArgumentNullException(nameof(deleteChatUseCase));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

    }

    [HttpPost("/Chat/Create")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _createChatUseCase.Execute(request);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Chat);
    }
    //[HttpPost("create-private")]
    //public async Task<IActionResult> CreatePrivateChat([FromBody] CreateChatRequest request)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    var response = await _createChatUseCase.Execute(request);

    //    if (!response.IsSuccess)
    //        return BadRequest(response.ErrorMessage);

    //    return Ok(new
    //    {
    //        response.Chat.ChatId,
    //        ChatName = await _chatRepository.GetChatNameForUser(
    //            User.FindFirstValue(ClaimTypes.NameIdentifier),
    //            response.Chat.ChatId)
    //    });
    //}
    [HttpPost("AddUsersToChat")]
    [Authorize]
    public async Task<IActionResult> AddUsersToChatByPhones([FromBody] AddUserToChatRequest request)
    {
        // Получаем UserId из claims
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }
        var addedUsers = new List<string>(); // To store added user IDs
        var errorMessages = new List<string>(); // To store error messages
                                                // Перебираем список телефонных номеров
        foreach (var phoneNumber in request.PhoneNumbers)
        {
            // 1. Находим пользователя по номеру телефона
            var user = await _userRepository.GetUserByPhoneNumber(phoneNumber);
            if (user == null)
            {
                _logger.LogWarning($"User with phone number {phoneNumber} not found.");
                errorMessages.Add($"User with phone number {phoneNumber} not found.");
                continue; // Skip to the next phone number
            }
            // 2. Добавляем пользователя в чат
            var response = await _addUserToChatUseCase.Execute(request.ChatId, user.Id);
            if (!response.IsSuccess)
            {
                _logger.LogError($"Error adding user {user.Id} to chat {request.ChatId}: {response.Message}");
                errorMessages.Add($"Error adding user {user.Id} to chat {request.ChatId}: {response.Message}");
            }
            else
            {
                addedUsers.Add(user.Id); // Add the user ID to the list of added users
            }
        }
        if (errorMessages.Any())
        {
            return BadRequest(string.Join(" ", errorMessages)); // Return all error messages
        }
        return Ok(addedUsers); // Return the list of added user IDs
    }

    [HttpPost("InviteToChat")]
    [Authorize]
    public async Task<IActionResult> InviteToChat([FromBody] InviteToChatRequest request)
    {
        var inviterUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(inviterUserId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }
        _logger.LogInformation($"ChatId value: {request.ChatId}");
        foreach (var invitedUserId in request.InvitedUserIds)
        {
            // Проверяем, существует ли пользователь с указанным invitedUserId
            var user = await _userRepository.GetById(invitedUserId);
            if (user == null)
            {
                _logger.LogWarning($"User with id {invitedUserId} not found.");
                return BadRequest($"User with id {invitedUserId} not found.");
            }
            var notificationDto = new NotificationDto
            {
                UserId = invitedUserId,
                Message = $"Вас пригласили в чат {request.ChatId} от {inviterUserId}. Принять или отклонить?",
                Timestamp = DateTime.UtcNow,
                IsRead = false,
                NotificationType = NotificationType.ChatInvite,
                ChatId = request.ChatId
            };
            var notificationResponse = await _sendNotificationUseCase.Execute(notificationDto, inviterUserId); // Send invite notif
            if (!notificationResponse.IsSuccess)
            {
                return BadRequest(notificationResponse.ErrorMessage);
            }
        }
        return Ok("Приглашение в чат отправлено.");
    }
    [HttpPost("RespondToInvite")]
    [Authorize]
    public async Task<IActionResult> RespondToInvite([FromBody] ChatInviteResponseRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }
        _logger.LogInformation($"RespondToInvite: NotificationId = {request.NotificationId}, Accepted = {request.Accepted}");
        var response = await _respondToChatInviteUseCase.Execute(request.NotificationId, userId, request.Accepted);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.SuccessMessage);
    }
    [HttpGet("UserChats")] //получение чата пользователя
    [Authorize]
    public async Task<IActionResult> GetUserChats()
    {
        // Получаем UserId из claims
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        // Используем Use Case для получения чатов пользователя
        var response = await _getUserChatsUseCase.Execute(userId);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Chats);
    }
    [HttpPost("SetChatName")] //изменение название чата
    public async Task<IActionResult> SetChatName(string chatId, string chatName)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        await _chatService.SetChatNameForUser(userId, chatId, chatName);

        return Ok();
    }

    [HttpGet("GetChatUsersList")]
    public async Task<IActionResult> List(string chatId) //  Принимаем ChatId в качестве параметра
    {
        _logger.LogInformation($"ChatController.List: Attempting to retrieve chat list for ChatId = {chatId}.");

        // Получаем UserId из claims
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ChatController.List: UserId not found in claims.");
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        // Используем Use Case для получения списка участников чата
        var response = await _getChatListUseCase.Execute(chatId, userId); //  Передаем ChatId и UserId

        if (!response.IsSuccess)
        {
            _logger.LogError($"ChatController.List: Failed to retrieve chat list: {response.ErrorMessage}");
            return BadRequest(response.ErrorMessage);
        }

        _logger.LogInformation($"ChatController.List: Successfully retrieved chat list for ChatId = {chatId}.");
        return Ok(response.Users); //  Возвращаем список UserDto
    }
    [HttpGet("GetChatNameForUser")]
    public async Task<IActionResult> GetChatName(string chatId)
    {
        _logger.LogInformation($"ChatController.GetChatName: Attempting to retrieve chat name for ChatId = {chatId}.");

        // Получаем UserId из claims
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ChatController.GetChatName: UserId not found in claims.");
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        try
        {
            //  Получаем название чата из сервиса
            var chatName = await _chatService.GetChatNameForUser(userId, chatId);

            if (chatName == null)
            {
                _logger.LogWarning($"ChatController.GetChatName: Chat name not found for ChatId = {chatId} and UserId = {userId}.");
                return NotFound($"Chat name not found for ChatId = {chatId} and UserId = {userId}.");
            }

            _logger.LogInformation($"ChatController.GetChatName: Successfully retrieved chat name for ChatId = {chatId} and UserId = {userId}.");
            return Ok(chatName); //  Возвращаем название чата
        }
        catch (Exception ex)
        {
            _logger.LogError($"ChatController.GetChatName: An error occurred: {ex.Message}");
            return StatusCode(500, $"An error occurred while retrieving chat name: {ex.Message}");
        }
    }
    [HttpPost("SendMessage")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }
        var response = await _sendMessageUseCase.Execute(request, userId);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response);
    }
    [HttpPost("UpdateMessageStatus")]
    [Authorize]
    public async Task<IActionResult> UpdateMessageStatus([FromBody] UpdateMessageStatusRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _updateMessageStatusUseCase.Execute(request, userId);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok();
    }
    [HttpGet("GetMessages")]
    [Authorize]
    public async Task<IActionResult> GetMessages([FromQuery] GetChatMessagesRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        if (!await _chatRepository.IsUserInChat(request.ChatId, userId))
        {
            return Unauthorized();
        }
        var response = await _getChatMessagesUseCase.Execute(request, userId);

        return Ok(response);
    }
    [HttpGet("GetLastChatMessage")]
    [Authorize]
    public async Task<IActionResult> GetLastChatMessage([FromQuery] string chatId)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var response = await _getLastChatMessageUseCase.Execute(chatId, userId);

        return Ok(response);
    }
    [HttpDelete("RemoveUserFromChat")]
    [Authorize]
    public async Task<IActionResult> RemoveUserFromChat(string chatId, string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var response = await _removeUserFromChatUseCase.Execute(chatId, userId, currentUserId);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok("User removed from chat successfully.");
    }
    [HttpDelete("DeleteChat")]
    public async Task<IActionResult> DeleteChat(string chatId)
    {
        _logger.LogInformation($"ChatController.DeleteChat: Attempting to delete chat with ID {chatId}.");

        try
        {
            // 1. Проверка входных данных (убедитесь, что chatId не null и не пустой)
            if (string.IsNullOrEmpty(chatId))
            {
                _logger.LogError("ChatController.DeleteChat: Invalid input - chatId is null or empty.");
                return BadRequest("chatId не может быть пустым.");
            }

            // 2. Вызов Use Case для удаления чата
            var response = await _deleteChatUseCase.Execute(chatId);

            if (!response.IsSuccess)
            {
                _logger.LogError($"ChatController.DeleteChat: Failed to delete chat: {response.ErrorMessage}");
                return BadRequest(response.ErrorMessage);
            }

            _logger.LogInformation($"ChatController.DeleteChat: Chat with ID {chatId} deleted successfully.");
            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            _logger.LogError($"ChatController.DeleteChat: An unexpected error occurred: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please check the server logs.");
        }
    }
    [HttpDelete("DeleteMessage")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        _logger.LogInformation($"ChatController.DeleteMessage: Attempting to delete message with ID {messageId}.");

        try
        {
            // 1. Validate input
            if (messageId <= 0)
            {
                _logger.LogError("ChatController.DeleteMessage: Invalid input data - MessageId is invalid.");
                return BadRequest("Неверные входные данные: MessageId должен быть больше 0.");
            }

            // 2. Delete message
            await _chatRepository.DeleteMessage(messageId);

            _logger.LogInformation($"ChatController.DeleteMessage: Message with ID {messageId} deleted successfully.");
            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            _logger.LogError($"ChatController.DeleteMessage: An error occurred while deleting message with ID {messageId}: {ex.Message}");
            return StatusCode(500, $"An error occurred while deleting message: {ex.Message}");
        }
    }
}