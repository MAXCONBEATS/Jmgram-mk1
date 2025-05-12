using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Storage;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface INotificationRepository
    {

        Task<List<NotificationDto>> GetNotificationsByUserId(int userId);
        Task AddNotification(Notification notification);
        Task<NotificationDto?> GetNotificationById(int notificationId);
        Task UpdateNotification(Notification notification);
        Task DeleteNotification(int notificationId);
    }


    public class InMemoryNotificationRepository : INotificationRepository
    {
        private readonly List<Notification> _notifications = new List<Notification>();
        private readonly JMgramDbContext _context;
        public InMemoryNotificationRepository(JMgramDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<NotificationDto>> GetNotificationsByUserId(int userId)
        {
            return _notifications
                .Where(n => n.userId == userId)
                .Select(MapToDto)
                .ToList();
        }

        public async Task AddNotification(Notification notification)
        {
            notification.Id = _notifications.Count > 0 ? _notifications.Max(n => n.Id) + 1 : 1;
            _notifications.Add(notification);
            await Task.CompletedTask;
        }

        public async Task<NotificationDto?> GetNotificationById(int notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            return notification == null ? null : MapToDto(notification);
        }

        public async Task UpdateNotification(Notification notification)
        {

            // Find the notification in the correct DbSet based on its type
            Notification? existingNotification = null;

            if (notification is MessageNotification messageNotification)
            {
                existingNotification = await _context.MessageNotifications.FindAsync(notification.Id);
            }
            else if (notification is ContactRequestNotification contactRequestNotification)
            {
                existingNotification = await _context.ContactRequestNotifications.FindAsync(notification.Id);
            }
            else if (notification is SystemNotification systemNotification)
            {
                existingNotification = await _context.SystemNotifications.FindAsync(notification.Id);
            }

            if (existingNotification == null)
            {
                throw new ArgumentException($"Notification with id {notification.Id} not found");
            }

            // Update common properties
            existingNotification.userId = notification.userId;
            existingNotification.message = notification.message;
            if (notification.messageId is not null)
            {
                existingNotification.messageId = notification.messageId;
            }
            existingNotification.isRead = notification.isRead;

            // Update properties specific to the type
            if (existingNotification is MessageNotification existingMessageNotification && notification is MessageNotification newmessageNotification)
            {
                existingMessageNotification.NotificationMessageId = newmessageNotification.NotificationMessageId;
            }
            else if (existingNotification is ContactRequestNotification existingContactRequestNotification && notification is ContactRequestNotification newContactRequestNotification)
            {
                existingContactRequestNotification.contactRequestId = newContactRequestNotification.contactRequestId;
                existingContactRequestNotification.senderUserId = newContactRequestNotification.senderUserId;
            }
            else if (existingNotification is SystemNotification existingSystemNotification && notification is SystemNotification newSystemNotification)
            {
                existingSystemNotification.source = newSystemNotification.source;
            }
            else
            {
                // Handle the case where the notification type doesn't match
                throw new ArgumentException($"Cannot update notification with id {notification.Id} because the type of the existing notification does not match the type of the new notification.");

                // Or, implement logic to replace the existing notification with a new one of the correct type
            }

            // Save changes
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNotification(int notificationId)
        {
            var notificationToRemove = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notificationToRemove != null)
            {
                _notifications.Remove(notificationToRemove);
                await Task.CompletedTask;
            }
            else
            {
                throw new ArgumentException($"Notification with id {notificationId} not found");
            }
        }

        private NotificationDto MapToDto(Notification notification)
        {
            if (notification is MessageNotification messageNotification)
            {
                return new MessageNotificationDto
                {
                    Id = messageNotification.Id,
                    UserId = messageNotification.userId,
                    Message = messageNotification.message,
                    MessageId = messageNotification.NotificationMessageId,
                    Timestamp = messageNotification.timestamp,
                    IsRead = messageNotification.isRead,
                    NotificationType = NotificationType.Message,
                };
            }
            else if (notification is ContactRequestNotification contactRequestNotification)
            {
                return new ContactRequestNotificationDto
                {
                    Id = contactRequestNotification.Id,
                    UserId = contactRequestNotification.userId,
                    Message = contactRequestNotification.message,
                    MessageId = contactRequestNotification.messageId,
                    Timestamp = contactRequestNotification.timestamp,
                    IsRead = contactRequestNotification.isRead,
                    ContactRequestId = contactRequestNotification.contactRequestId,
                    SenderUserId = contactRequestNotification.senderUserId,
                    NotificationType = NotificationType.ContactRequest,
                };
            }
            else if (notification is SystemNotification systemNotification)
            {
                return new SystemNotificationDto
                {
                    Id = systemNotification.Id,
                    UserId = systemNotification.userId,
                    Message = systemNotification.message,
                    MessageId = systemNotification.messageId,
                    Timestamp = systemNotification.timestamp,
                    IsRead = systemNotification.isRead,
                    Source = systemNotification.source,
                    NotificationType = NotificationType.System,
                };
            }
            else
            {
                return new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.userId,
                    Message = notification.message,
                    MessageId = notification.messageId,
                    Timestamp = notification.timestamp,
                    IsRead = notification.isRead,
                };
            }
        }
    }
}
