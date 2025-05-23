using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    [Table("ChatUsers")]
    public class ChatUser
    {
        [Key, Column(Order = 0)]
        [MaxLength(100)]
        public string ChatId { get; set; }

        [Key, Column(Order = 1)]
        [MaxLength(450)]
        public string UserId { get; set; }

        public DateTime JoinedAt { get; set; }

        public string ChatName { get; set; }

        [ForeignKey("ChatId")]
        public Chat Chat { get; set; }

        [ForeignKey("UserId")]
        public AppIdentityUser User { get; set; }
    }

}
