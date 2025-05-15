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
    private readonly CreateChatUseCase _createChatUseCase;
    private readonly AddUserToChatUseCase _addUserToChatUseCase;
    private readonly GetChatListUseCase _getChatListUseCase;  
    private readonly SendMessageUseCase _sendMessageUseCase;
    private readonly UpdateMessageStatusUseCase _updateMessageStatusUseCase;
    private readonly GetChatMessagesUseCase _getChatMessagesUseCase;

    public ChatController(ILogger<ChatController> logger, CreateChatUseCase createChatUseCase, AddUserToChatUseCase addUserToChatUseCase, 
        IHttpContextAccessor httpContextAccessor, GetChatListUseCase getChatListUseCase, SendMessageUseCase sendMessageUseCase, 
        UpdateMessageStatusUseCase updateMessageStatusUseCase, GetChatMessagesUseCase getChatMessagesUseCase, IChatRepository chatRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        _createChatUseCase = createChatUseCase ?? throw new ArgumentNullException(nameof(createChatUseCase));
        _addUserToChatUseCase = addUserToChatUseCase ?? throw new ArgumentNullException(nameof(addUserToChatUseCase));
        _getChatListUseCase = getChatListUseCase ?? throw new ArgumentNullException(nameof(getChatListUseCase));    
        _sendMessageUseCase = sendMessageUseCase ?? throw new ArgumentNullException(nameof(sendMessageUseCase));
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

        // Вызываем AddUserToChatUseCase
        var response = await _addUserToChatUseCase.Execute(request, userId); // Передаём UserId в UseCase

        if (!response.IsSuccess)
        {
            return BadRequest(response.Message);
        }

        return Ok(response.ChatUsers);
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