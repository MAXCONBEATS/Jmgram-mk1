using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public UserDto? User { get; set; }
    }

}
