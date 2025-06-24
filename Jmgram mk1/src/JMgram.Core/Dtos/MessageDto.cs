namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string ChatId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string Status { get; set; }
    }
    public class MessageForSendingDto
    {
        public string ChatId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
    }

}
