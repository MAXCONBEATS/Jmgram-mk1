namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
