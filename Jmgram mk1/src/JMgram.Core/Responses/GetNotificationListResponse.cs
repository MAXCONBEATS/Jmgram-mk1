using Jmgram_mk1.src.JMgram.Core.Dtos;

namespace Jmgram_mk1.src.JMgram.Core.Responses
{
    public class GetNotificationListResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public List<NotificationDto>? Notifications { get; set; }
    }
}




