import React, { useEffect, useState } from 'react';
import '../css/NotificationWindow.css';
import { markAsRead } from '../controllers/NotificationController';

function NotificationWindow({ notifications, onRemoveNotification }) {
  const [closedNotifications, setClosedNotifications] = useState(new Set());

  useEffect(() => {
    if (notifications.length === 0) return;

    const timers = notifications.map(notification => {
      console.log('Notification id for auto-dismiss:', notification.id);
      return setTimeout(() => {
        onRemoveNotification(notification.id);
      }, 5000); // Auto dismiss after 5 seconds
    });

    return () => {
      timers.forEach(timer => clearTimeout(timer));
    };
  }, [notifications, onRemoveNotification]);

  const renderNotificationMessage = (notification) => {
    if (notification.notificationType === 'Message') {
      return `Новое сообщение: ${notification.message || '(нет текста)'}`;
    } else if (notification.notificationType === 'ContactRequest') {
      return `Запрос на добавление в контакты от пользователя ${notification.senderUserId || '(неизвестный пользователь)'}`;
    } else if (notification.notificationType === 'System') {
      return `Системное уведомление: ${notification.message || '(нет текста)'}`;
    } else {
      return notification.message || '(нет текста)';
    }
  };

  const handleMarkAsRead = async (id) => {
    try {
      const success = await markAsRead(id);
      if (success) {
        setClosedNotifications(prev => new Set(prev).add(id));
        onRemoveNotification(id);
      }
    } catch (error) {
      console.error('Ошибка при пометке уведомления как прочитанного:', error);
    }
  };

  const handleClose = (id) => {
    setClosedNotifications(prev => new Set(prev).add(id));
    onRemoveNotification(id);
  };

  return (
    <div className="notification-window">
      {notifications.filter(n => !closedNotifications.has(n.id)).map((notification, index) => (
        <div key={`${notification.id}-${index}`} className="notification-item">
          <div className="notification-message">{renderNotificationMessage(notification)}</div>
          <button className="notification-close" onClick={() => handleClose(notification.id)}>×</button>
          <button className="notification-read" onClick={() => handleMarkAsRead(notification.id)}>Прочитал</button>
        </div>
      ))}
    </div>
  );
}

export default NotificationWindow;
