using Jmgram_mk1.src.JMgram.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class ContactRequestDto
    {
        public Guid Id { get; set; }
        public string? SenderUserId { get; set; }
        public string? RecipientUserId { get; set; }
        public ContactRequestStatus Status { get; set; }
    }
}
