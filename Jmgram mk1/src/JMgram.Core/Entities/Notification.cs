using System.ComponentModel.DataAnnotations;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public abstract class Notification
    {
        [Key]
        public int Id { get; set; }
        public int userId { get; set; }
        public string message { get; set; } = "";
        public int? messageId { get; set; }
        public DateTime timestamp { get; set; }
        public bool isRead { get; set; }
        // Remove this line: public NotificationType notificationType { get; set; }
    }

    public class MessageNotification : Notification
    {
        public int NotificationMessageId { get; set; }

    }

    public class ContactRequestNotification : Notification
    {
        public int contactRequestId { get; set; }
        public int senderUserId { get; set; }
        public new string message { get; set; } // Опциональное приветственное сообщение

    }

    public class SystemNotification : Notification
    {
        public new string message { get; set; }
        public string source { get; set; } // От кого пришло системное уведомление (например, "Telegram Bot")

    }

    public enum NotificationType
    {
        Message, ContactRequest, System
    }
}