using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class ChatInvitation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string ChatId { get; set; }
        public string? SenderUserId { get; set; }
        public string? RecipientUserId { get; set; }
        public ChatInvationStatus Status { get; set; }
        public virtual Chat? Chat { get; set; }
        public virtual AppIdentityUser? SenderUser { get; set; }
        public virtual AppIdentityUser? RecipientUser { get; set; }

    }
    public enum ChatInvationStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }
}
