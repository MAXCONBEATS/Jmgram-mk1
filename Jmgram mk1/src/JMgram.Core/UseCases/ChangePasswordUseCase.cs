
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public interface IChangePasswordUseCase
    {
        Task<ChangePasswordResponse> Execute(ChangePasswordRequest request);
    }

    public class ChangePasswordUseCase : IChangePasswordUseCase
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ILogger<ChangePasswordUseCase> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangePasswordUseCase(
            UserManager<AppIdentityUser> userManager,
            ILogger<ChangePasswordUseCase> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<ChangePasswordResponse> Execute(ChangePasswordRequest request)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogError("UserId not found in claims");
                    return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "Unauthorized" };
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning($"User not found for UserId: {userId}");
                    return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "User not found" };
                }

                var checkPasswordResult = await _userManager.CheckPasswordAsync(user, request.OldPassword);

                if (!checkPasswordResult)
                {
                    _logger.LogWarning($"Invalid old password for UserId: {userId}");
                    return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "Invalid old password." };
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    _logger.LogError($"Error changing password for UserId: {userId}");
                    foreach (var error in changePasswordResult.Errors)
                    {
                        _logger.LogError(error.Description);
                    }
                    return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "Error changing password" };
                }

                var userDto = new UserDto
                {
                    Id = userId,
                    Phone = user.PhoneNumber,
                    Profile = new UserProfileDto
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                    }
                };

                return new ChangePasswordResponse { IsSuccess = true, User = userDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "Internal server error" };
            }
        }
    }

}
