namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class ChatInviteResponseRequest
    {
        public int NotificationId { get; set; } // ID уведомления
        public bool Accepted { get; set; } // True, если принял, false - отклонил
    }
}
