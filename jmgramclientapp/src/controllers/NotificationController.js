import axios from 'axios';
axios.defaults.baseURL = 'https://localhost:5087';

export async function getNotifications() {
  try {
    const response = await axios.get('https://localhost:5087/Notification/GetNotifications', {
      withCredentials: true
    });
    return response.data;
  } catch (error) {
    console.error('Ошибка при получении уведомлений:', error);
    throw error;
  }
}

export async function sendNotification(notificationDto) {
  try {
    const response = await axios.post('https://localhost:5087/Notification/Send', notificationDto, { withCredentials: true });
    return response.data;
  } catch (error) {
    console.error('Ошибка при отправке уведомления:', error);
    throw error;
  }
}

export async function deleteNotification(id) {
  try {
    const response = await axios.delete(`https://localhost:5087/Notification/Delete?id=${id}`, { withCredentials: true });
    return response.status === 204;
  } catch (error) {
    console.error('Ошибка при удалении уведомления:', error);
    throw error;
  }
}
