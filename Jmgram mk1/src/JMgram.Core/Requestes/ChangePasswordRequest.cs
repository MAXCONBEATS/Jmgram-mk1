using System.ComponentModel.DataAnnotations;

namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class ChangePasswordRequest
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }





}
