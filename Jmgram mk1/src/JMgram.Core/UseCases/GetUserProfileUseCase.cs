

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class GetUserProfileUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserProfileUseCase> _logger;

        public GetUserProfileUseCase(IUserRepository userRepository, ILogger<GetUserProfileUseCase> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetUserProfileResponse> Execute(GetUserProfileRequest request)
        {
            // 1. Валидация входных данных
            if (request == null)
            {
                _logger.LogError("GetUserProfileRequest is null.");
                return new GetUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null.",
                    Profile = null
                };
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                _logger.LogError("UserId is null or empty.");
                return new GetUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "UserId cannot be null or empty.",
                    Profile = null
                };
            }

            try
            {
                _logger.LogInformation($"Attempting to retrieve user profile with UserId: {request.UserId}");
                var userProfile = await _userRepository.GetUserProfileById(request.UserId);

                if (userProfile == null)
                {
                    _logger.LogWarning($"UserProfile with UserId {request.UserId} not found.");
                    return new GetUserProfileResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"UserProfile with UserId {request.UserId} not found.",
                        Profile = null
                    };
                }

                _logger.LogInformation($"UserProfile retrieved successfully for UserId: {request.UserId}");

                // 4. Формирование DTO
                var profileDto = new UserProfileDto
                {
                    UserId = userProfile.UserId,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    AvatarPath = userProfile.AvatarPath,
                    Bio = userProfile.Bio,
                    LastSeen = userProfile.LastSeen
                };

                // 5. Формирование успешного ответа
                return new GetUserProfileResponse
                {
                    IsSuccess = true,
                    Profile = profileDto
                };
            }
            catch (Exception ex)
            {
                // 6. Обработка ошибок
                _logger.LogError(ex, $"An error occurred while retrieving user profile for UserId: {request.UserId}");
                return new GetUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred while retrieving user profile: {ex.Message}",
                    Profile = null
                };
            }
        }
    }

}
