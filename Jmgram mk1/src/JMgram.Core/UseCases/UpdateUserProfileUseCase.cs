

using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class UpdateUserProfileUseCase
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserProfileUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UpdateUserProfileResponse> Execute(UpdateUserProfileRequest request)
        {
            // 1. Validation
            if (request == null)
            {
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Request cannot be null."
                };
            }

            if (request.Profile == null)
            {
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Profile cannot be null."
                };
            }

            if (request.Profile.UserId <= 0)
            {
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "UserId must be greater than 0."
                };
            }

            try
            {
                var existingProfile = await _userRepository.GetUserProfileById(request.Profile.UserId);

                if (existingProfile == null)
                {
                    return new UpdateUserProfileResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Profile with userId doesn't exists."
                    };
                }


                // 3. Update Profile with updateProfile DTO method
                existingProfile.FirstName = request.Profile.FirstName;
                existingProfile.LastName = request.Profile.LastName;
                existingProfile.AvatarPath = request.Profile.AvatarPath;
                existingProfile.Bio = request.Profile.Bio;
                existingProfile.LastSeen = DateTime.UtcNow;


                // 4. Save changes via repo. Убедитесь что в userRepository есть _userRepository.UpdateProfile method
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
                return new UpdateUserProfileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"An error occurred updating user profile: {ex.Message}"
                };
            }
        }
    }

}
