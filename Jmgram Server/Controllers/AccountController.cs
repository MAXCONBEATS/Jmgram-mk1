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
using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Jmgram_mk1.src.JMgram.Core.Responses;
[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly SignInManager<AppIdentityUser> _signInManager;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<AppIdentityUser> userManager, SignInManager<AppIdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpPost]
    public async Task<Results<Ok, ValidationProblem>> Register(
    [FromBody] RegisterUserRequest registration,
    JMgramDbContext context,
    UserManager<AppIdentityUser> userManager,
    SignInManager<AppIdentityUser> signInManager)
    {
        // Check if user with the same phone number already exists
        var existingUser = await userManager.FindByNameAsync(registration.Phone);
        if (existingUser != null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            { "Phone", new[] { "Phone number already exists." } }
        });
        }

        try
        {
            var user = new AppIdentityUser
            {
                UserName = registration.Phone,
                Email = registration.Phone,
                PhoneNumber = registration.Phone,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Phone = registration.Phone
            };

            var result = await userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description });
                return TypedResults.ValidationProblem(errors);
            }

            await userManager.AddClaimAsync(user, new Claim(ClaimTypes.NameIdentifier, user.Id));

            var userProfile = new UserProfile
            {
                UserId = user.Id,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Phone = registration.Phone, //добавлено
                LastSeen = DateTime.UtcNow
            };

            context.UserProfiles.Add(userProfile);

            // Сохраняем изменения
            await context.SaveChangesAsync();

            await signInManager.SignInAsync(user, isPersistent: false);

            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            { "error", new[] { ex.Message } }
        });
        }
    }
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        _logger.LogInformation($"Login attempt for phone: {login.Phone}");

        var user = await _userManager.FindByNameAsync(login.Phone);

        if (user == null)
        {
            _logger.LogWarning($"User not found for phone: {login.Phone}");
            return Unauthorized("Invalid phone number or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation($"User {login.Phone} successfully logged in with Id: {user.Id}");

            //  Используйте SignInManager.SignInAsync
            await _signInManager.SignInAsync(user, isPersistent: true); // Используем SignInManager

            _logger.LogInformation("User successfully signed in");

            return Ok();
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning($"Account locked out for user: {login.Phone}");
            return Unauthorized("Account locked out");
        }

        if (result.IsNotAllowed)
        {
            _logger.LogWarning($"Login is not allowed for user: {login.Phone}");
            return Unauthorized("Login is not allowed");
        }

        if (result.RequiresTwoFactor)
        {
            _logger.LogWarning($"Two factor authentication required for user: {login.Phone}");
            return Unauthorized("Requires two factor authentication");
        }

        _logger.LogWarning($"Invalid password for user: {login.Phone}");
        return Unauthorized("Invalid phone number or password");
    }
    [HttpGet("Me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfoResponse>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Создаем DTO для возврата, а не используем entity напрямую
        return Ok(new UserInfoResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber
            // Добавьте другие нужные поля
        });
    }
    [HttpPost]
    public IActionResult Authenticated()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized(); // Возвращаем 401 Unauthorized
        }

        return Ok(); // Возвращаем 200 OK
    }
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete(".AspNetCore.Identity.Application");
        return Ok();
    }

    private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
    TypedResults.ValidationProblem(new Dictionary<string, string[]> {
                { errorCode, [errorDescription] }
    });

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }
}



