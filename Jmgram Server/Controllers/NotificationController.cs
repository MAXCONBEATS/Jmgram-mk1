using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly SendNotificationUseCase _sendNotificationUseCase;
    private readonly GetNotificationListUseCase _getNotificationListUseCase;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(SendNotificationUseCase sendNotificationUseCase, GetNotificationListUseCase getNotificationListUseCase,
        ILogger<NotificationController> logger, IHttpContextAccessor httpContextAccessor)
    {
        _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
        _getNotificationListUseCase = getNotificationListUseCase ?? throw new ArgumentNullException(nameof(getNotificationListUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    [HttpPost("Send")]
    public async Task<IActionResult> Send([FromBody] NotificationDto notificationDto)
    {
        if (notificationDto == null)
        {
            _logger.LogError("NotificationController.Send: Notification data is required.");
            return BadRequest("Notification data is required.");
        }

        var senderUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); // Get sender's UserId

        if (string.IsNullOrEmpty(senderUserId))
        {
            _logger.LogError("NotificationController.Send: Unable to retrieve UserId from claims.");
            return Unauthorized("Unable to retrieve UserId from claims.");
        }

        _logger.LogInformation($"NotificationController.Send: Attempting to send notification from UserId: {senderUserId}");

        var response = await _sendNotificationUseCase.Execute(notificationDto, senderUserId); // Pass senderUserId

        if (!response.IsSuccess)
        {
            _logger.LogError($"NotificationController.Send: Error sending notification: {response.ErrorMessage}");
            return BadRequest(response.ErrorMessage);
        }

        _logger.LogInformation("NotificationController.Send: Notification sent successfully.");
        return Ok("Notification sent successfully.");
    }


    [HttpGet("GetNotifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("UserId is required.");
        }

        var response = await _getNotificationListUseCase.Execute(userId);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Notifications);
    }
}