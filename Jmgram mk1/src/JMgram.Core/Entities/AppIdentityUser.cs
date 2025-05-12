using Microsoft.AspNetCore.Identity;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class AppIdentityUser : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
    }
}
