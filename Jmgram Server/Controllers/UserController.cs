using Microsoft.AspNetCore.Mvc;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Jmgram_mk1.src.JMgram.Core.Repositories;


[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly GetUserProfileUseCase _getUserProfileUseCase;
    private readonly IChangePasswordUseCase _changePasswordUseCase;
    private readonly UpdateUserProfileUseCase _updateUserProfileUseCase;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;

    public UserController(GetUserProfileUseCase getUserProfileUseCase, IChangePasswordUseCase changePasswordUseCase, UpdateUserProfileUseCase updateUserProfileUseCase, ILogger<UserController> logger, IUserRepository userRepository)
    {
        _getUserProfileUseCase = getUserProfileUseCase ?? throw new ArgumentNullException(nameof(getUserProfileUseCase));
        _changePasswordUseCase = changePasswordUseCase ?? throw new ArgumentNullException(nameof(changePasswordUseCase));
        _updateUserProfileUseCase = updateUserProfileUseCase ?? throw new ArgumentNullException(nameof(updateUserProfileUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpGet("/User/GetProfile")]
    [Authorize]
    public async Task<IActionResult> GetProfile(string? userId = null)
    {
        string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(currentUserId))
        {
            _logger.LogError("Unable to retrieve user ID from claims.");
            return Unauthorized("Unable to retrieve user ID from claims.");
        }

        if (string.IsNullOrEmpty(userId))
        {
            userId = currentUserId;
        }

        var request = new GetUserProfileRequest { UserId = userId };

        var response = await _getUserProfileUseCase.Execute(request);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Profile);
    }
    [Authorize]
    [HttpPatch("/User/UpdateProfile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request, [FromServices] JMgramDbContext _context)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var response = await _updateUserProfileUseCase.Execute(request);
        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response);

    }
    [HttpPost("/User/ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _changePasswordUseCase.Execute(request);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response);
    }
    [HttpGet("/User/Search")]
    public async Task<IActionResult> Search([FromQuery] string phone)
    {
        var user = await _userRepository.GetUserByPhoneNumber(phone);

        if (user == null)
        {
            return NotFound("Пользователь не найден.");
        }

        return Ok(new
        {
            id = user.Id,
            firstName = user.FirstName,
            lastName = user.LastName,
            phone = user.Phone
        });
    }

}