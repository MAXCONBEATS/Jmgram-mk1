 // src/controllers/ChatController.js
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