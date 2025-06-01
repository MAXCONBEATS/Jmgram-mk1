using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jmgram_mk1.src.JMgram.Core.Entities
{
    public abstract class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public string? ChatId { get; set; }
        public NotificationType NotificationType { get; set; }
        public virtual AppIdentityUser User { get; set; }
    }

    public class MessageNotification : Notification
    {
        public int MessageId { get; set; }

    }

    public class SystemNotification : Notification
    {
        public string Source { get; set; }
    }

    public class ContactRequestNotification : Notification
    {
        public string SenderUserId { get; set; }
    }
    public class ChatInviteNotification : Notification
    {      
    }

    public enum NotificationType
    {
        Message = 0,
        ContactRequest = 1,
        System = 2,
        ChatInvite = 3
    }
}