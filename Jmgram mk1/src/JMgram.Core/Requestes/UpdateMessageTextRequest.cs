using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class UpdateMessageTextRequest
    {
        public int MessageId { get; set; }
        public string? Text { get; set; }
    }
}
