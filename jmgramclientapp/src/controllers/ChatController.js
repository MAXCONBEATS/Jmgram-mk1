 // src/controllers/ChatController.js
 import axios from 'axios';
 
 export async function getUserChats() {
  const response = await axios.get('http://127.0.0.1:5087/Chat/UserChats', { withCredentials: true });
  return response.data;
 }
 
  export async function getLastChatMessage(chatId) {
 try {
  const response = await axios.get(`http://localhost:5087/Chat/GetLastChatMessage`, {
   withCredentials: true,
   params: {
    chatId: chatId,
   },
  });
  return response.data; //  Предполагаем, что API возвращает LastChatMessageDto
 } catch (error) {
  console.error(`Ошибка при получении последнего сообщения для чата ${chatId}:`, error);
  return null;
 }
}