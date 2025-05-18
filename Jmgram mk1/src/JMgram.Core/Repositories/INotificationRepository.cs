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
        Task<NotificationDto?> GetNotificationDtoById(int notificationId); // Renamed method
        Task<Notification> GetNotificationById(int notificationId);
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
             .Where(n => n.UserId == userId)
             .OrderByDescending(n => n.Timestamp)
             .ToListAsync();

            return notifications.Select(MapToDto).ToList();
        }

        public async Task AddNotification(Notification notification)
        {
            notification.Timestamp = DateTime.UtcNow;
            notification.IsRead = false;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<NotificationDto?> GetNotificationDtoById(int notificationId) // Renamed method
        {
            var notification = await _context.Notifications
             .FirstOrDefaultAsync(n => n.Id == notificationId);

            return notification == null ? null : MapToDto(notification);
        }
        public async Task<Notification> GetNotificationById(int notificationId)
        {
            return await _context.Notifications.FindAsync(notificationId);
        }
        public async Task UpdateNotification(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotification(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                UserId = notification.UserId,
                Message = notification.Message,
                Timestamp = notification.Timestamp,
                IsRead = notification.IsRead,
                NotificationType = Enum.Parse<NotificationType>(notification.GetType().Name.Replace("Notification", ""))
            };
        }
    }
}
