using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Services;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class LoginUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public LoginUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<LoginResponse> Execute(LoginRequest request)
        {
            // 1.  Проверить входные данные
            if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Номер телефона и пароль должны быть заполнены." };
            }

            // 2.  Найти пользователя по номеру телефона
            var user = await _userRepository.GetByPhone(request.Phone);
            if (user == null)
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Пользователь с таким номером телефона не найден." };
            }

            // 3.  Проверить пароль
            var passwordVerificationResult = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);

            if (passwordVerificationResult == PasswordVerificationResult.Success)
            {
                // 4.  Аутентификация успешна
                UserDto userDto = new UserDto  // Map User to DTO
                {
                    Id = user.Id.ToString(),
                    Phone = user.Phone
                };

                return new LoginResponse { IsSuccess = true, User = userDto };
            }
            else
            {
                // 5.  Неверный пароль
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Неверный пароль." };
            }
        }
    }
}
