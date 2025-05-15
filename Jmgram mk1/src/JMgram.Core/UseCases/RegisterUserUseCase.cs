

using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Jmgram_mk1.src.JMgram.Core.Repositories;
using Jmgram_mk1.src.JMgram.Core.Services;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public class RegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<RegisterUserResponse> Execute(RegisterUserRequest request)
        {
            // 1. Проверка, что номер телефона не занят
            if (await _userRepository.IsPhoneTaken(request.Phone))
            {
                return new RegisterUserResponse { IsSuccess = false, ErrorMessage = "Номер телефона уже занят." };
            }

            // 2. Хеширование пароля
            string passwordHash = _passwordHasher.HashPassword(request.Password);

            // 3. Создание нового объекта User и UserProfile из Request
            var user = new User
            {
                Phone = request.Phone,    
                PasswordHash = passwordHash, 
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Profile = new UserProfile
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                }
            };

            // 4. Сохранение пользователя в базе данных
            await _userRepository.Add(user);

            // Преобразование User в UserDto
            UserDto userDto = MapUserToDto(user);

            return new RegisterUserResponse { IsSuccess = true, User = userDto };
        }

        private UserDto MapUserToDto(User user)
        {
            if (user == null)
            {
                return null; // Или выбросить исключение ArgumentNullException
            }

            return new UserDto
            {
                Id = user.Id.ToString(),  
                Phone = user.Phone,
            };
        }
    }
}
