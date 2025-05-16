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

    public NotificationController(SendNotificationUseCase sendNotificationUseCase, GetNotificationListUseCase getNotificationListUseCase, IHttpContextAccessor httpContextAccessor)
    {
        _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
        _getNotificationListUseCase = getNotificationListUseCase ?? throw new ArgumentNullException(nameof(getNotificationListUseCase));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    [HttpPost("Send")]
    public async Task<IActionResult> Send([FromBody] NotificationDto notificationDto)
    {
        if (notificationDto == null)
        {
            return BadRequest("Notification data is required.");
        }

        var response = await _sendNotificationUseCase.Execute(notificationDto);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

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