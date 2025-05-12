namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public Chat Chat { get; set; }
        public User Sender { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
    }
    public enum MessageStatus
    {
        Sent,       // Отправлено, но не доставлено
        Delivered,  // Отправлено и доставлено
        Read        // Отправлено, доставлено и прочитано
    }

}
