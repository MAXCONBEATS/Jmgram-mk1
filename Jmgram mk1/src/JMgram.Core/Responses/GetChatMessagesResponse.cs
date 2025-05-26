using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
   public class GetChatMessagesResponse
    {
        public List<MessageDto> Chat { get; set; }
        public int TotalMessages { get; set; }
        public int TotalPages { get; set; }
    }





}
