using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;
namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class CreateChatResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public ChatDto Chat { get; set; }
        public string CreatorUserId { get; set; }
    }





}
