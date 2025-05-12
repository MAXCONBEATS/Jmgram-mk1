namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class ContactDto
    {
        public int Id { get; set; }
        public int UserId { get; set; } // ID пользователя, которому принадлежит контакт
        public int ContactUserId { get; set; } // ID пользователя, который является контактом
        public DateTime AddedAt { get; set; }
    }
}
