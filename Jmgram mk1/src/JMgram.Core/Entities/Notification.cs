using System.ComponentModel.DataAnnotations;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public abstract class Notification
    {
        [Key]
        public int Id { get; set; }
        public string userId { get; set; }
        public string message { get; set; } = "";
        public DateTime timestamp { get; set; }
        public bool isRead { get; set; }
    }

    public class MessageNotification : Notification
    {
        public int MessageId { get; set; }

    }

    public class ContactRequestNotification : Notification
    {
        public string senderUserId { get; set; }
        public new string message { get; set; } // Опциональное приветственное сообщение

    }

    public class SystemNotification : Notification
    {
        public new string message { get; set; }
        public string source { get; set; } // От кого пришло системное уведомление (например, "Telegram Bot")

    }

    public enum NotificationType
    {
        Message = 0,
        ContactRequest = 1,
        System = 2
    }
}