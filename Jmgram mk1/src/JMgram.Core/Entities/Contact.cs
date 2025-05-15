using System.ComponentModel.DataAnnotations;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } // Изменено на string
        public string Name { get; set; }
        public string Phone { get; set; }

        // Свойства для связи с AppIdentityUser в качестве контактного лица
        public string ContactUserId { get; set; }
        public AppIdentityUser ContactUser { get; set; }

        public AppIdentityUser User { get; set; }
    }
}
