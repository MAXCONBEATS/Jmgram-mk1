using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Repositories;
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
    private readonly INotificationRepository _notificationRepository;

    public NotificationController(SendNotificationUseCase sendNotificationUseCase, GetNotificationListUseCase getNotificationListUseCase, INotificationRepository notificationRepository,
        ILogger<NotificationController> logger, IHttpContextAccessor httpContextAccessor)
    {
        _sendNotificationUseCase = sendNotificationUseCase ?? throw new ArgumentNullException(nameof(sendNotificationUseCase));
        _getNotificationListUseCase = getNotificationListUseCase ?? throw new ArgumentNullException(nameof(getNotificationListUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    }

    [HttpPost("Send")]
    public async Task<IActionResult> Send([FromBody] NotificationDto notificationDto)
    {
        if (notificationDto == null)
        {
            _logger.LogError("NotificationController.Send: Notification data is required.");
            return BadRequest("Notification data is required.");
        }

        var senderUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(senderUserId))
        {
            _logger.LogError("NotificationController.Send: Unable to retrieve UserId from claims.");
            return Unauthorized("Unable to retrieve UserId from claims.");
        }

        _logger.LogInformation($"NotificationController.Send: Attempting to send notification from UserId: {senderUserId}");

        var response = await _sendNotificationUseCase.Execute(notificationDto, senderUserId);

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
        _logger.LogInformation("NotificationController.GetNotifications: Attempting to retrieve notifications.");

        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("NotificationController.GetNotifications: UserId not found in claims.");
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        try
        {
            var response = await _getNotificationListUseCase.Execute(userId);

            if (!response.IsSuccess)
            {
                _logger.LogError($"NotificationController.GetNotifications: Failed to retrieve notifications: {response.ErrorMessage}");
                return BadRequest(response.ErrorMessage);
            }

            _logger.LogInformation($"NotificationController.GetNotifications: Successfully retrieved notifications for user {userId}.");
            return Ok(response.Notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError($"NotificationController.GetNotifications: An error occurred: {ex.Message}");
            return StatusCode(500, $"An error occurred while retrieving notifications: {ex.Message}");
        }
    }
    [HttpPut("MarkAsRead")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        _logger.LogInformation($"NotificationController.MarkAsRead: Attempting to mark notification with ID {notificationId} as read.");

        try
        {
            if (notificationId <= 0)
            {
                _logger.LogError("NotificationController.MarkAsRead: Invalid input data - notificationId is invalid.");
                return BadRequest("Неверные входные данные: notificationId должен быть больше 0.");
            }
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("NotificationController.MarkAsRead: UserId not found in claims.");
                return Unauthorized("Не удалось получить UserId из claims.");
            }

            await _notificationRepository.MarkNotificationAsRead(notificationId);

            _logger.LogInformation($"NotificationController.MarkAsRead: Notification with ID {notificationId} marked as read successfully.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"NotificationController.MarkAsRead: An error occurred while marking notification with ID {notificationId} as read: {ex.Message}");
            return StatusCode(500, $"An error occurred while marking notification as read: {ex.Message}");
        }
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        _logger.LogInformation($"NotificationController.DeleteNotification: Attempting to delete notification with ID {id}.");

        try
        {
            await _notificationRepository.DeleteNotification(id);
            _logger.LogInformation($"NotificationController.DeleteNotification: Notification with ID {id} deleted successfully.");
            return NoContent(); 
        }
        catch (Exception ex)
        {
            _logger.LogError($"NotificationController.DeleteNotification: An error occurred while deleting notification with ID {id}: {ex.Message}, Inner Exception: {ex.InnerException}");
            return BadRequest("Failed to delete notification."); 
        }
    }
}