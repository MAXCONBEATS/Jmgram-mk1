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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        // 1. Пытаемся получить ID пользователя из Claims
        if (!TryGetUserId(out int userId))
        {
            return Unauthorized("Invalid user ID.");
        }

        // 2. Формирование запроса
        var request = new GetUserProfileRequest { UserId = userId };

        // 3. Вызов use case
        var response = await _getUserProfileUseCase.Execute(request);

        // 4. Обработка результата
        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Profile);
    }


    // Вспомогательный метод для получения идентификатора пользователя из Claims
    private bool TryGetUserId(out int userId)
    {
        userId = 0; // Initialize userId

        var userIdClaim = User.FindFirst(ClaimTypes.Sid); // Изменено: используем ClaimTypes.Sid

        if (userIdClaim == null)
        {
            _logger.LogWarning("User ID Claim not found.");
            return false;
        }

        if (!int.TryParse(userIdClaim.Value, out userId))
        {
            _logger.LogError("Failed to parse user id from claim: {UserIdClaimValue}", userIdClaim.Value);
            return false;
        }

        return true;
    }
}