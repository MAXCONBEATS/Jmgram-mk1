namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class ChatUserDto
    {
        public string ChatId { get; set; }
        public string UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public string ChatName { get; set; }
        public MessageDto LastMessage { get; set; }
    }

}
