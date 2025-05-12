namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class ChatUser
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public Chat Chat { get; set; }
        public User User { get; set; }
    }

}
