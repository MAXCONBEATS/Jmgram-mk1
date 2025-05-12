
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class ChangePasswordUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ChangePasswordUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<ChangePasswordResponse> Execute(ChangePasswordRequest request)
        {
            // 1. Проверить входные данные
            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "Неверные входные данные." };
            }

            // 2. Получить пользователя по ID
            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
            {
                return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "Пользователь не найден." };
            }

            // 3. Проверить старый пароль
            var passwordVerificationResult = _passwordHasher.VerifyPassword(user.PasswordHash, request.OldPassword);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return new ChangePasswordResponse { IsSuccess = false, ErrorMessage = "Неверный старый пароль." };
            }

            // 4. Хешировать новый пароль
            string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            // 5. Обновить пароль в базе данных
            user.PasswordHash = newPasswordHash;
            await _userRepository.Update(user);

            // 6. Создать DTO (если нужно вернуть информацию о пользователе)
            UserDto userDto = new UserDto
            {
                Id = user.Id,
                Phone = user.Phone
            };

            // 7. Вернуть результат
            return new ChangePasswordResponse { IsSuccess = true, User = userDto };
        }
    }

}
