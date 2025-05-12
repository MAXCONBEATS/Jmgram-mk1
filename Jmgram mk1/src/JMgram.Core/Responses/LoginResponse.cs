using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; } // Сообщение об ошибке, если вход не удался
        public UserDto? User { get; set; } // DTO пользователя, если вход успешен
    }

}
