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


[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly GetUserProfileUseCase _getUserProfileUseCase;
    private readonly IChangePasswordUseCase _changePasswordUseCase;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;

    public UserController(GetUserProfileUseCase getUserProfileUseCase, IChangePasswordUseCase changePasswordUseCase, ILogger<UserController> logger, IUserRepository userRepository)
    {
        _getUserProfileUseCase = getUserProfileUseCase ?? throw new ArgumentNullException(nameof(getUserProfileUseCase));
        _changePasswordUseCase = changePasswordUseCase ?? throw new ArgumentNullException(nameof(changePasswordUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpGet("/User/GetProfile")]
    [Authorize]
    public async Task<IActionResult> GetProfile(string? userId = null)
    {
        string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // получаем id текущего пользователя

        if (string.IsNullOrEmpty(currentUserId))
        {
            _logger.LogError("Unable to retrieve user ID from claims.");
            return Unauthorized("Unable to retrieve user ID from claims.");
        }
        // Если userId не указан, получаем профиль текущего пользователя
        if (string.IsNullOrEmpty(userId))
        {
            userId = currentUserId;
        }

        // Формирование запроса
        var request = new GetUserProfileRequest { UserId = userId };

        // Вызов use case
        var response = await _getUserProfileUseCase.Execute(request);

        // Обработка результата
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
        if (request?.Profile == null)
        {
            return BadRequest("Profile data is required.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        var userProfile = await _context.UserProfiles.FindAsync(userId);

        if (userProfile == null)
        {
            return NotFound();
        }

        // Обновляем поля профиля пользователя
        if (request.Profile.FirstName is not null)
            userProfile.FirstName = request.Profile.FirstName;

        if (request.Profile.LastName is not null)
            userProfile.LastName = request.Profile.LastName;

        if (request.Profile.AvatarPath is not null)
            userProfile.AvatarPath = request.Profile.AvatarPath;

        if (request.Profile.Bio is not null)
            userProfile.Bio = request.Profile.Bio;

        userProfile.LastSeen = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new UpdateUserProfileResponse { IsSuccess = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new UpdateUserProfileResponse { IsSuccess = false, ErrorMessage = "Internal server error" });
        }
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

        return Ok(response); // Возвращаем response в случае успеха
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