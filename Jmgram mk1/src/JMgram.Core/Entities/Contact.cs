using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class Contact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }

        public string ContactUserId { get; set; }
        public AppIdentityUser ContactUser { get; set; }

        public AppIdentityUser User { get; set; }
    }
}
