namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class GetChatMessagesRequest
    {
        public required string ChatId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }



}
