using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Storage;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface INotificationRepository
    {
        Task<List<NotificationDto>> GetNotificationsByUserId(string userId);
        Task AddNotification(Notification notification);
        Task<NotificationDto?> GetNotificationById(int notificationId);
        Task UpdateNotification(Notification notification);
        Task DeleteNotification(int notificationId);
    }
    public class NotificationRepository : INotificationRepository
    {
        private readonly JMgramDbContext _context;

        public NotificationRepository(JMgramDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<NotificationDto>> GetNotificationsByUserId(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.userId == userId)
                .OrderByDescending(n => n.timestamp)
                .ToListAsync();

            return notifications.Select(MapToDto).ToList();
        }

        public async Task AddNotification(Notification notification)
        {
            notification.timestamp = DateTime.UtcNow;
            notification.isRead = false;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<NotificationDto?> GetNotificationById(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            return notification == null ? null : MapToDto(notification);
        }

        public async Task UpdateNotification(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotification(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        private NotificationDto MapToDto(Notification notification)
        {
            switch (notification)
            {
                case MessageNotification msg:
                    return new MessageNotificationDto
                    {
                        UserId = msg.userId,
                        Message = msg.message,
                        MessageId = msg.MessageId,
                        Timestamp = msg.timestamp,
                        IsRead = msg.isRead,
                        NotificationType = NotificationType.Message
                    };

                case ContactRequestNotification cr:
                    return new ContactRequestNotificationDto
                    {
                        UserId = cr.userId,
                        Message = cr.message,
                        Timestamp = cr.timestamp,
                        IsRead = cr.isRead,
                        SenderUserId = cr.senderUserId,
                        NotificationType = NotificationType.ContactRequest
                    };

                case SystemNotification sys:
                    return new SystemNotificationDto
                    {
                        UserId = sys.userId,
                        Message = sys.message,
                        Timestamp = sys.timestamp,
                        IsRead = sys.isRead,
                        Source = sys.source,
                        NotificationType = NotificationType.System
                    };

                default:
                    return new NotificationDto
                    {
                        UserId = notification.userId,
                        Message = notification.message,
                        Timestamp = notification.timestamp,
                        IsRead = notification.isRead
                    };
            }
        }
    }
}
