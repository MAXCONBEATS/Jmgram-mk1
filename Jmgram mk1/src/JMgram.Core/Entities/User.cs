using System.ComponentModel.DataAnnotations;
namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
        public UserProfile Profile { get; set; }
        public ICollection<Contact> Contacts { get; set; }
        public ICollection<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();

    }
}
