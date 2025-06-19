using Jmgram_mk1.src.JMgram.Core.Entities;

namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class ChatInvationDto
    {
        public Guid Id { get; set; }
        public string ChatId {  get; set; }
        public string? SenderUserId { get; set; }
        public string? RecipientUserId { get; set; }
        public ContactRequestStatus Status { get; set; }
    }
}
