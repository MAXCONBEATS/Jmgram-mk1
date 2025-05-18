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
    private readonly GetChatListUseCase _getChatListUseCase;  
    private readonly SendMessageUseCase _sendMessageUseCase;
    private readonly SendNotificationUseCase _sendNotificationUseCase;
    private readonly RespondToChatInviteUseCase _respondToChatInviteUseCase;
    private readonly UpdateMessageStatusUseCase _updateMessageStatusUseCase;
    private readonly GetChatMessagesUseCase _getChatMessagesUseCase;

    public ChatController(ILogger<ChatController> logger, CreateChatUseCase createChatUseCase, AddUserToChatUseCase addUserToChatUseCase, 
        IHttpContextAccessor httpContextAccessor, GetChatListUseCase getChatListUseCase, SendMessageUseCase sendMessageUseCase, SendNotificationUseCase sendNotificationUseCase,
    UpdateMessageStatusUseCase updateMessageStatusUseCase, GetChatMessagesUseCase getChatMessagesUseCase,
    RespondToChatInviteUseCase respondToChatInviteUseCase,IChatRepository chatRepository, IUserRepository userRepository)
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
    [HttpPost("List")]
    [Authorize]
    public async Task<IActionResult> GetChatList([FromBody] GetChatListRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        var response = await _getChatListUseCase.Execute(request, userId); // Передаем UserId в UseCase

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Users);
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
}