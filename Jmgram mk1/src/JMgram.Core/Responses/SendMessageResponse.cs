using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Entities;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class SendMessageResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int Id { get; set; }
    }
}




