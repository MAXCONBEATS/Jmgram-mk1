using System.ComponentModel.DataAnnotations;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class UserProfile
    {
        [Key]
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? AvatarPath { get; set; }
        public string? Bio { get; set; }
        public DateTime LastSeen { get; set; }
        public User? User { get; set; }

    }

}
