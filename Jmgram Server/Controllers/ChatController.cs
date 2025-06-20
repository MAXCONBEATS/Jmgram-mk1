using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.AspNetCore.Mvc;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Services;
using NuGet.Packaging.Signing;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatInvationRepository _chatInvationRepository;
    private readonly CreateChatUseCase _createChatUseCase;
    private readonly CreateChatInvitationUseCase _createChatInvitationUseCase;
    private readonly AddUserToChatUseCase _addUserToChatUseCase;
    private readonly GetChatInvitationsUseCase _getChatInvitationsUseCase;
    private readonly IGetChatListUseCase _getChatListUseCase;
    private readonly ISendMessageUseCase _sendMessageUseCase;
    private readonly SendNotificationUseCase _sendNotificationUseCase;
    private readonly RespondToChatInviteUseCase _respondToChatInviteUseCase;
    private readonly UpdateMessageStatusUseCase _updateMessageStatusUseCase;
    private readonly GetChatMessagesUseCase _getChatMessagesUseCase;
    private readonly GetUserChatsUseCase _getUserChatsUseCase;
    private readonly IGetLastChatMessageUseCase _getLastChatMessageUseCase;
    private readonly UpdateMessageTextUseCase _updateMessageTextUseCase;
    private readonly RemoveUserFromChatUseCase _removeUserFromChatUseCase;
    private readonly DeleteChatUseCase _deleteChatUseCase;
    private readonly IChatService _chatService;


    public ChatController(ILogger<ChatController> logger, CreateChatUseCase createChatUseCase, AddUserToChatUseCase addUserToChatUseCase,
     IHttpContextAccessor httpContextAccessor, IGetChatListUseCase getChatListUseCase, GetChatInvitationsUseCase getChatInvitationsUseCase, UpdateMessageTextUseCase updateMessageTextUseCase,
     ISendMessageUseCase sendMessageUseCase, SendNotificationUseCase sendNotificationUseCase, IGetLastChatMessageUseCase getLastChatMessageUseCase, CreateChatInvitationUseCase createChatInvitationUseCase,
     UpdateMessageStatusUseCase updateMessageStatusUseCase, GetChatMessagesUseCase getChatMessagesUseCase, GetUserChatsUseCase getUserChatsUseCase, IChatService chatService,
     RemoveUserFromChatUseCase removeUserFromChatUseCase, DeleteChatUseCase deleteChatUseCase,
     RespondToChatInviteUseCase respondToChatInviteUseCase, IChatRepository chatRepository, IUserRepository userRepository, IChatInvationRepository chatInvationRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _chatInvationRepository = chatInvationRepository ?? throw new ArgumentNullException(nameof(chatInvationRepository));
        _createChatUseCase = createChatUseCase ?? throw new ArgumentNullException(nameof(createChatUseCase));
        _addUserToChatUseCase = addUserToChatUseCase ?? throw new ArgumentNullException(nameof(addUserToChatUseCase));
        _getChatListUseCase = getChatListUseCase ?? throw new ArgumentNullException(nameof(getChatListUseCase));
        _getChatInvitationsUseCase = getChatInvitationsUseCase ?? throw new ArgumentNullException(nameof(getChatInvitationsUseCase));
        _sendMessageUseCase = sendMessageUseCase ?? throw new ArgumentNullException(nameof(sendMessageUseCase));
        _createChatInvitationUseCase = createChatInvitationUseCase ?? throw new ArgumentNullException(nameof(createChatInvitationUseCase));
        _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
        _respondToChatInviteUseCase = respondToChatInviteUseCase ?? throw new ArgumentNullException(nameof(respondToChatInviteUseCase));
        _updateMessageStatusUseCase = updateMessageStatusUseCase ?? throw new ArgumentNullException(nameof(updateMessageStatusUseCase));
        _updateMessageTextUseCase = updateMessageTextUseCase ?? throw new ArgumentNullException(nameof(updateMessageTextUseCase));
        _getChatMessagesUseCase = getChatMessagesUseCase ?? throw new ArgumentNullException(nameof(getChatMessagesUseCase));
        _getUserChatsUseCase = getUserChatsUseCase ?? throw new ArgumentNullException(nameof(getUserChatsUseCase));       
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

    [HttpPost("AddUsersToChat")]
    [Authorize]
    public async Task<IActionResult> AddUsersToChatByPhones([FromBody] AddUserToChatRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }
        var addedUsers = new List<string>();
        var errorMessages = new List<string>();
        foreach (var phoneNumber in request.PhoneNumbers)
        {
            var user = await _userRepository.GetUserByPhoneNumber(phoneNumber);
            if (user == null)
            {
                _logger.LogWarning($"User with phone number {phoneNumber} not found.");
                errorMessages.Add($"User with phone number {phoneNumber} not found.");
                continue;
            }
            var response = await _addUserToChatUseCase.Execute(request.ChatId, user.Id);
            if (!response.IsSuccess)
            {
                _logger.LogError($"Error adding user {user.Id} to chat {request.ChatId}: {response.Message}");
                errorMessages.Add($"Error adding user {user.Id} to chat {request.ChatId}: {response.Message}");
            }
            else
            {
                addedUsers.Add(user.Id);
            }
        }
        if (errorMessages.Any())
        {
            return BadRequest(string.Join(" ", errorMessages));
        }
        return Ok(addedUsers);
    }

    [HttpPost("InviteToChat")]
    [Authorize]
    public async Task<IActionResult> InviteToChat([FromBody] CreateChatInvitationRequest request)
    {
        var inviterUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(inviterUserId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }
        var response = await _createChatInvitationUseCase.Execute(request.ChatId, request.SenderId, request.RecipientId);

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
        _logger.LogInformation($"RespondToInvite: NotificationId = {request.ChatInvitationId}, Accepted = {request.Accepted}");
        var response = await _respondToChatInviteUseCase.Execute(request.ChatInvitationId, userId, request.Accepted);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.SuccessMessage);
    }
    [HttpGet("ChatInvites")]
    [Authorize]
    public async Task<IActionResult> GetChatInvitations()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        var response = await _getChatInvitationsUseCase.Execute(userId);
        if (!response.IsSuccess)
        {
            _logger.LogError($"ChatController.GetContactRequests: Failed to retrieve contact requests: {response.ErrorMessage}");
            return BadRequest(response.ErrorMessage);
        }
        var incomingRequestsDto = response.IncomingRequests.Select(cr => new ChatInvationDto
        {
            Id = cr.Id,
            ChatId = cr.ChatId,
            SenderUserId = cr.SenderUserId,
            RecipientUserId = cr.RecipientUserId,
            Status = cr.Status
        }).ToList();

        return Ok(new
        {
            IncomingRequests = incomingRequestsDto
        });
    }
    [HttpGet("UserChats")]
    [Authorize]
    public async Task<IActionResult> GetUserChats()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        var response = await _getUserChatsUseCase.Execute(userId);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Chats);
    }
    [HttpPost("SetChatName")] 
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
    public async Task<IActionResult> List(string chatId) 
    {
        _logger.LogInformation($"ChatController.List: Attempting to retrieve chat list for ChatId = {chatId}.");

        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ChatController.List: UserId not found in claims.");
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        var response = await _getChatListUseCase.Execute(chatId, userId);

        if (!response.IsSuccess)
        {
            _logger.LogError($"ChatController.List: Failed to retrieve chat list: {response.ErrorMessage}");
            return BadRequest(response.ErrorMessage);
        }

        _logger.LogInformation($"ChatController.List: Successfully retrieved chat list for ChatId = {chatId}.");
        return Ok(response.Users);
    }
    [HttpGet("GetChatNameForUser")]
    public async Task<IActionResult> GetChatName(string chatId)
    {
        _logger.LogInformation($"ChatController.GetChatName: Attempting to retrieve chat name for ChatId = {chatId}.");

        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ChatController.GetChatName: UserId not found in claims.");
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        try
        {
            var chatName = await _chatService.GetChatNameForUser(userId, chatId);

            if (chatName == null)
            {
                _logger.LogWarning($"ChatController.GetChatName: Chat name not found for ChatId = {chatId} and UserId = {userId}.");
                return NotFound($"Chat name not found for ChatId = {chatId} and UserId = {userId}.");
            }

            _logger.LogInformation($"ChatController.GetChatName: Successfully retrieved chat name for ChatId = {chatId} and UserId = {userId}.");
            return Ok(chatName);
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
    [HttpPatch("UpdateMessageText")]
    [Authorize]
    public async Task<IActionResult> UpdateMessageText([FromBody]UpdateMessageTextRequest updateMessageTextRequest)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var response = await _updateMessageTextUseCase.Execute(updateMessageTextRequest);
        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

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
            if (string.IsNullOrEmpty(chatId))
            {
                _logger.LogError("ChatController.DeleteChat: Invalid input - chatId is null or empty.");
                return BadRequest("chatId не может быть пустым.");
            }

            var response = await _deleteChatUseCase.Execute(chatId);

            if (!response.IsSuccess)
            {
                _logger.LogError($"ChatController.DeleteChat: Failed to delete chat: {response.ErrorMessage}");
                return BadRequest(response.ErrorMessage);
            }

            _logger.LogInformation($"ChatController.DeleteChat: Chat with ID {chatId} deleted successfully.");
            return NoContent();
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
            if (messageId <= 0)
            {
                _logger.LogError("ChatController.DeleteMessage: Invalid input data - MessageId is invalid.");
                return BadRequest("Неверные входные данные: MessageId должен быть больше 0.");
            }

            await _chatRepository.DeleteMessage(messageId);

            _logger.LogInformation($"ChatController.DeleteMessage: Message with ID {messageId} deleted successfully.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ChatController.DeleteMessage: An error occurred while deleting message with ID {messageId}: {ex.Message}");
            return StatusCode(500, $"An error occurred while deleting message: {ex.Message}");
        }
    }
}