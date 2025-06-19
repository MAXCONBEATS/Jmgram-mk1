using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class CreateChatInvitationRequest
    {
        public string ChatId { get; set; }
        public string? SenderId { get; set; }
        public string? RecipientId { get; set; }
    }
}
