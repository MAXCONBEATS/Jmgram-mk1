namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class ChatUser
    {
        public string ChatId { get; set; }
        public string UserId { get; set; } // Id пользователя string
        public DateTime JoinedAt { get; set; }
        public Chat Chat { get; set; }
        public AppIdentityUser User { get; set; }
    }

}
