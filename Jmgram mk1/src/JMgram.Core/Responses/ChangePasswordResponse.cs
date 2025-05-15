using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;
namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class ChangePasswordResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public UserDto? User { get; set; }
    }






}
