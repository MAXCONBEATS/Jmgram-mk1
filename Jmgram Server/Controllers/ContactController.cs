using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController] 
[Route("[controller]")]
[Authorize]
public class ContactController : ControllerBase
{
    private readonly CreateContactRequestUseCase _createContactRequestUseCase;
    private readonly AcceptContactRequestUseCase _acceptContactRequestUseCase;
    private readonly UpdateContactNameUseCase _updateContactNameUseCase;
    private readonly DeleteContactUseCase _deleteContactUseCase;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ContactController> _logger;

    public ContactController(CreateContactRequestUseCase createContactRequestUseCase, AcceptContactRequestUseCase acceptContactRequestUseCase, UpdateContactNameUseCase updateContactNameUseCase, DeleteContactUseCase deleteContactUseCase, IHttpContextAccessor httpContextAccessor, ILogger<ContactController> logger)
    {
        _createContactRequestUseCase = createContactRequestUseCase ?? throw new ArgumentNullException(nameof(createContactRequestUseCase));
        _acceptContactRequestUseCase = acceptContactRequestUseCase ?? throw new ArgumentNullException(nameof(acceptContactRequestUseCase));
        _updateContactNameUseCase = updateContactNameUseCase ?? throw new ArgumentNullException(nameof(updateContactNameUseCase));
        _deleteContactUseCase = deleteContactUseCase ?? throw new ArgumentNullException(nameof(deleteContactUseCase));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] AddContactRequest request)
    {
        var senderUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(senderUserId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        _logger.LogInformation($"ContactController.Add: Adding contact request for SenderUserId: {senderUserId}, RecipientUserId: {request.ContactUserId}");
        var response = await _createContactRequestUseCase.Execute(senderUserId, request.ContactUserId);

        if (!response.IsSuccess)
        {
            _logger.LogError($"ContactController.Add: Error creating contact request: {response.ErrorMessage}");
            return BadRequest(response.ErrorMessage);
        }

        _logger.LogInformation($"ContactController.Add: Contact request created successfully.");
        return Ok("Запрос на добавление в друзья отправлен.");
    }

    [HttpPost("Accept")]
    public async Task<IActionResult> Accept([FromBody] AcceptContactRequest request)
    {
        _logger.LogInformation($"ContactController.Accept: Accepting contact request with ID: {request.ContactRequestId}");

        var result = await _acceptContactRequestUseCase.Execute(request.ContactRequestId);

        if (result)
        {
            _logger.LogInformation($"ContactController.Accept: Contact request with ID {request.ContactRequestId} accepted successfully.");
            return Ok("Запрос на добавление в друзья принят.");
        }
        else
        {
            _logger.LogError($"ContactController.Accept: Failed to accept contact request with ID {request.ContactRequestId}.");
            return BadRequest("Не удалось принять запрос на добавление в друзья.");
        }
    }

    [HttpPost("UpdateName")]
    public async Task<IActionResult> UpdateName([FromBody] UpdateContactNameRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        _logger.LogInformation($"ContactController.UpdateName: Updating contact name for UserId: {userId}, ContactUserId: {request.ContactUserId} to Name: {request.NewName}");

        var result = await _updateContactNameUseCase.Execute(userId, request.ContactUserId, request.NewName);

        if (result)
        {
            _logger.LogInformation($"ContactController.UpdateName: Contact name updated successfully for UserId: {userId}, ContactUserId: {request.ContactUserId}");
            return Ok("Имя контакта успешно изменено.");
        }
        else
        {
            _logger.LogError($"ContactController.UpdateName: Failed to update contact name for UserId: {userId}, ContactUserId: {request.ContactUserId}.");
            return BadRequest("Не удалось изменить имя контакта.");
        }
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteContactRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Не удалось получить UserId из claims.");
        }

        _logger.LogInformation($"ContactController.Delete: Deleting contact for UserId: {userId}, ContactUserId: {request.ContactUserId}");

        var result = await _deleteContactUseCase.Execute(userId, request.ContactUserId);

        if (result)
        {
            _logger.LogInformation($"ContactController.Delete: Contact deleted successfully for UserId: {userId}, ContactUserId: {request.ContactUserId}");
            return Ok("Контакт успешно удален.");
        }
        else
        {
            _logger.LogError($"ContactController.Delete: Failed to delete contact for UserId: {userId}, ContactUserId: {request.ContactUserId}.");
            return BadRequest("Не удалось удалить контакт.");
        }
    }
}