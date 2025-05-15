using Jmgram_mk1.src.JMgram.Core.Entities;

namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class UpdateMessageStatusRequest
    {
        public int MessageId { get; set; }
        public string NewStatus { get; set; }
        public string ChatId { get; set; } // Добавлено
    }

}
