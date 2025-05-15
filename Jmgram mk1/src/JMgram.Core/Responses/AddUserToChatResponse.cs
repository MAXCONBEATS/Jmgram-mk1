using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;
namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class AddUserToChatResponse
    {
        public bool IsSuccess { get; set; }
        public List<ChatUserDto>? ChatUsers { get; set; }
        public string? Message { get; set; }
    }
}




