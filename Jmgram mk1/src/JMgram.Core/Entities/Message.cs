namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public class Message
    {
        public int Id { get; init; }
        public string ChatId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public Chat Chat { get; set; }
        public AppIdentityUser Sender { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
    }
    public enum MessageStatus
    {
        Sent,       
        Delivered,  
        Read        
    }

}
