

using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.Extensions.Logging;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class UpdateUserProfileUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateUserProfileUseCase> _logger;

        public UpdateUserProfileUseCase(IUserRepository userRepository, ILogger<UpdateUserProfileUseCase> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UpdateUserProfileResponse> Execute(UpdateUserProfileRequest request)
        {
            // 1. Validation
            if (request == null)
            {
                _logger.LogError("UpdateUserProfileRequest cannot be null.");
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null."
                };
            }

            if (request.Profile == null)
            {
                _logger.LogError("Profile cannot be null.");
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Profile cannot be null."
                };
            }

            if (string.IsNullOrEmpty(request.Profile.UserId))
            {
                _logger.LogError("UserId cannot be null or empty.");
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "UserId cannot be null or empty."
                };
            }

            try
            {
                // 2. Get existing profile from repo
                var existingProfile = await _userRepository.GetUserProfileById(request.Profile.UserId);

                if (existingProfile == null)
                {
                    _logger.LogWarning($"Profile with UserId {request.Profile.UserId} doesn't exists.");
                    return new UpdateUserProfileResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Profile with userId doesn't exists."
                    };
                }

                // 3. Update Profile with updateProfile DTO method
                existingProfile.FirstName = request.Profile.FirstName ?? existingProfile.FirstName;
                existingProfile.LastName = request.Profile.LastName ?? existingProfile.LastName;
                existingProfile.AvatarPath = request.Profile.AvatarPath ?? existingProfile.AvatarPath;
                existingProfile.Bio = request.Profile.Bio ?? existingProfile.Bio;
                existingProfile.LastSeen = DateTime.UtcNow;

                // 4. Save changes via repo
                await _userRepository.UpdateProfile(existingProfile);

                // 5. Return success
                return new UpdateUserProfileResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                // 6. Catch errors
                _logger.LogError(ex, $"An error occurred updating user profile for UserId: {request.Profile.UserId}");
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred updating user profile: {ex.Message}"
                };
            }
        }

    }

}
