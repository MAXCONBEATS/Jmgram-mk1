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

    [HttpGet("get-profile")]
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

}