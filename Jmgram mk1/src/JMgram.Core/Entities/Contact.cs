namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public int ContactUserId { get; set; } // ID пользователя, который является контактом
        public DateTime AddedAt { get; set; }
        public User User { get; set; }
        public User ContactUser { get; set; } 
    }
}
