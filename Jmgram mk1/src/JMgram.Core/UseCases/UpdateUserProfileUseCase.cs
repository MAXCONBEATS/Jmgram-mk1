

using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public interface IUpdateUserProfileUseCase
    {
        Task<UpdateUserProfileResponse> Execute(UpdateUserProfileRequest request);
    }
    public class UpdateUserProfileUseCase : IUpdateUserProfileUseCase
    {
        private readonly JMgramDbContext _context;
        private readonly ILogger<UpdateUserProfileUseCase> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateUserProfileUseCase(
            JMgramDbContext context,
            ILogger<UpdateUserProfileUseCase> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UpdateUserProfileResponse> Execute(UpdateUserProfileRequest request)
        {
            try
            {
                // Получаем UserId из Claims через IHttpContextAccessor
                var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    _logger.LogError("UserId not found in claims");
                    return new UpdateUserProfileResponse { IsSuccess = false, ErrorMessage = "Unauthorized" };
                }

                var userProfile = await _context.UserProfiles.FindAsync(userId);

                if (userProfile == null)
                {
                    _logger.LogWarning($"Profile not found for UserId: {userId}");
                    return new UpdateUserProfileResponse { IsSuccess = false, ErrorMessage = "Profile not found" };
                }

                // Обновляем поля профиля пользователя
                if (request.Profile.FirstName is not null)
                    userProfile.FirstName = request.Profile.FirstName;

                if (request.Profile.LastName is not null)
                    userProfile.LastName = request.Profile.LastName;

                if (request.Profile.Bio is not null)
                    userProfile.Bio = request.Profile.Bio;

                // ... другие поля

                await _context.SaveChangesAsync();
                userProfile.LastSeen = DateTime.UtcNow; // Обновляем LastSeen только при успешном сохранении
                await _context.SaveChangesAsync();

                return new UpdateUserProfileResponse { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return new UpdateUserProfileResponse { IsSuccess = false, ErrorMessage = "Internal server error" };
            }
        }
    }
}
