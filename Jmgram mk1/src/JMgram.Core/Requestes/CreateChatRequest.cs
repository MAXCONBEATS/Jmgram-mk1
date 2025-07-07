using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class CreateChatRequest
    {
        public ChatDto Chat { get; set; }
        public List<string> Phones { get; set; }
    }



}
