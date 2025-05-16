using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class UpdateContactNameRequest
    {
        public required string ContactUserId { get; set; }
        public string? NewName { get; set; }
    }
}
