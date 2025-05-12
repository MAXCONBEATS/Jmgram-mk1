using Jmgram_mk1.src.JMgram.Core.Entities;
namespace Jmgram_mk1.src.JMgram.Core.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = "";
        public int? MessageId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    // DTO для уведомления о сообщении
    public class MessageNotificationDto : NotificationDto
    {
        public new int MessageId { get; set; }
    }

    // DTO для уведомления о запросе контакта
    public class ContactRequestNotificationDto : NotificationDto
    {
        public int ContactRequestId { get; set; }
        public int SenderUserId { get; set; }
    }

    // DTO для системного уведомления
    public class SystemNotificationDto : NotificationDto
    {
        public string Source { get; set; } = "";
    }


}
