import axios from 'axios';
axios.defaults.baseURL = 'https://localhost:5087';

export async function getUserChats() {
 const response = await axios.get('https://localhost:5087/Chat/UserChats', { withCredentials: true });
 return response.data;
}

export async function getLastChatMessage(chatId) {
 try {
  const response = await axios.get(`https://localhost:5087/Chat/GetLastChatMessage`, {
   withCredentials: true,
   params: {
    chatId: chatId,
   },
  });
  return response.data; // Предполагаем, что API возвращает LastChatMessageDto
 } catch (error) {
  console.error(`Ошибка при получении последнего сообщения для чата ${chatId}:`, error);
  return null;
 }
}

export async function createChat(request) {
 try {
  const response = await axios.post('https://localhost:5087/Chat/Create', request, { withCredentials: true });
  return response.data; // Предполагаем, что API возвращает данные созданного чата
 } catch (error) {
  console.error('Ошибка при создании чата:', error);
  throw error;
 }
}

export async function sendMessage(request) {
 try {
  // Wrap request in "message" and add timestamp if missing
  const messagePayload = {
   message: {
    chatId: request.chatId,
    text: request.text,
    timestamp: request.timestamp || new Date().toISOString(),
    senderId: request.senderId,
    senderName: request.senderName,
   }
  };
  const response = await axios.post('https://localhost:5087/Chat/SendMessage', messagePayload, { withCredentials: true });
  return response.data;
 } catch (error) {
  console.error('Ошибка при отправке сообщения:', error);
  throw error;
 }
}

export async function updateMessageStatus(request) {
 try {
  const response = await axios.post('https://localhost:5087/Chat/UpdateMessageStatus', request, { withCredentials: true });
  return response.data;
 } catch (error) {
  console.error('Ошибка при обновлении статуса сообщения:', error);
  throw error;
 }
}

export async function getMessages(chatId, pageNumber = 1, pageSize = 20) {
 try {
  const response = await axios.get('https://localhost:5087/Chat/GetMessages', {
   withCredentials: true,
   params: {
    ChatId: chatId,
    PageNumber: pageNumber,
    PageSize: pageSize,
   },
  });
  return response.data;
 } catch (error) {
  console.error('Ошибка при получении сообщений чата:', error);
  throw error;
 }
}

export async function deleteChat(chatId) {
 try {
  const response = await axios.delete('https://localhost:5087/Chat/DeleteChat', {
   withCredentials: true,
   params: {
    chatId: chatId,
   },
  });
  return response.data;
 } catch (error) {
  console.error('Ошибка при удалении чата:', error);
  throw error;
 }
}
