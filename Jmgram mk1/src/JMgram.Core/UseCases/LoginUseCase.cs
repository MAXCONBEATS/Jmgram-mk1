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
            // 1.  Валидация входных данных
            if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные." };
            }

            // 2.  Найти пользователя по номеру телефона
            var users = await _userRepository.GetByPhones(new List<string> { request.Phone });
            if (users == null || users.Count == 0)
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Пользователь с таким номером телефона не найден." };
            }

            var user = users[0]; // Получаем первого пользователя из списка
                                 // 3.  Проверить пароль
            var result = _passwordHasher.VerifyPassword(user.ToString(), request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Неверный пароль." };
            }

            // 4.  Сгенерировать JWT токен

            // 5.  Вернуть успешный ответ
            return new LoginResponse { IsSuccess = true }; // TODO: Вернуть JWT токен
        }
    }
}