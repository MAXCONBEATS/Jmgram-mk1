using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class ContactRequest
    {
        public int Id { get; set; }
        public string SenderUserId { get; set; }
        public string RecipientUserId { get; set; }
        public ContactRequestStatus Status { get; set; }
    }
    public enum ContactRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
