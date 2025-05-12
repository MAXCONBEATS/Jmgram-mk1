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


[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly GetUserProfileUseCase _getUserProfileUseCase;
    private readonly ILogger<UserController> _logger;

    public UserController(GetUserProfileUseCase getUserProfileUseCase, ILogger<UserController> logger)
    {
        _getUserProfileUseCase = getUserProfileUseCase ?? throw new ArgumentNullException(nameof(getUserProfileUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("GetProfile")]
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
    [HttpPatch("/UpdateProfile")]
    public async Task<IActionResult> UpdateProfile(
    [FromBody] UpdateUserProfileRequest request,
    [FromServices] ILogger<AccountController> _logger,
    [FromServices] IUpdateUserProfileUseCase _updateUserProfileUseCase) // Внедряем UseCase
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _updateUserProfileUseCase.Execute(request); // Вызываем UseCase

            if (response.IsSuccess)
            {
                return Ok(); // Или Ok(response), если хотите вернуть что-то еще
            }
            else
            {
                _logger.LogError(response.ErrorMessage);
                return StatusCode(500, response.ErrorMessage); // Или BadRequest, если это ошибка валидации
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "Internal server error");
        }
    }

}