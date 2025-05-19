using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class LastChatMessageDto
    {
        public string ChatId { get; set; }
        public string Text { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
