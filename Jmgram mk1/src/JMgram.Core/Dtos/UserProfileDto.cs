using System.ComponentModel.DataAnnotations;

namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class UserProfileDto
    {

        public string UserId { get; set; }
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, ErrorMessage = "Имя должно быть не более 50 символов")]
        public string FirstName { get; set; }
        [StringLength(50, ErrorMessage = "Фамилия должна быть не более 50 символов")]
        public string LastName { get; set; }
        public string? AvatarPath { get; set; }
        public string? Bio { get; set; }
        public DateTime LastSeen { get; set; }

    }

}
