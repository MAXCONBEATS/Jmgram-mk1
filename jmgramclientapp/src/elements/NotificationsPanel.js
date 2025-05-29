import React from 'react';
import { useNotifications } from '../hooks/useNotifications';
import NotificationWindow from './NotificationWindow';

function NotificationsPanel() {
  const {
    notifications,
    handleCloseNotification,
    handleMarkAsReadNotification,
  } = useNotifications();

  return (
    <NotificationWindow 
      notifications={notifications} 
      onCloseNotification={handleCloseNotification} 
      onMarkAsReadNotification={handleMarkAsReadNotification} 
    />
  );
}

export default NotificationsPanel;
