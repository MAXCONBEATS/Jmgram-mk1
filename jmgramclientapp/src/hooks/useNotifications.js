import { useState, useEffect } from 'react';
import { getNotifications, markAsRead } from '../controllers/NotificationController';

export function useNotifications() {
  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    const fetchNotifications = async () => {
      try {
        const data = await getNotifications();
        const unreadNotifications = data.filter(notification => !notification.isRead);
        setNotifications(unreadNotifications);
      } catch (error) {
        console.error('Ошибка при получении уведомлений:', error);
      }
    };
    fetchNotifications();
    const interval = setInterval(fetchNotifications, 30000);
    return () => clearInterval(interval);
  }, []);

  const handleCloseNotification = (id) => {
    setNotifications((prev) => prev.filter((notif) => notif.Id !== id));
  };

  const handleMarkAsReadNotification = async (id) => {
    try {
      const success = await markAsRead(id);
      if (success) {
        setNotifications((prev) => prev.filter((notif) => notif.Id !== id));
      }
    } catch (error) {
      console.error('Ошибка при пометке уведомления как прочитанного:', error);
    }
  };

  return {
    notifications,
    handleCloseNotification,
    handleMarkAsReadNotification,
  };
}
