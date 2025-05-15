using Microsoft.AspNetCore.Identity;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class AppIdentityUser : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
        public virtual UserProfile? UserProfile { get; set; } // Добавлено свойство
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>(); // Добавлено свойство
    }
}
