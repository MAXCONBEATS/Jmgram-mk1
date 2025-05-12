using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class SendMessageResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public MessageDto Message { get; set; }
    }
}




