namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
        public UserProfileDto Profile { get; set; }


    }
}
