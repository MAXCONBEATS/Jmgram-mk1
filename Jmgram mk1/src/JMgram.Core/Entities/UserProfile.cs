using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class UserProfile
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; init; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string? AvatarPath { get; set; }
        public string? Bio { get; set; }
        public DateTime LastSeen { get; set; }

        public virtual AppIdentityUser User { get; set; }
    }

}
