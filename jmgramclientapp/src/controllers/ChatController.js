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
  return response.data;
 } catch (error) {
  console.error(`Ошибка при получении последнего сообщения для чата ${chatId}:`, error);
  return null;
 }
}

export async function createChat(request) {
 try {
  const response = await axios.post('https://localhost:5087/Chat/Create', request, { withCredentials: true });
  return response.data; 
 } catch (error) {
  console.error('Ошибка при создании чата:', error);
  throw error;
 }
}

export async function sendMessage(request) {
 try {
  const messagePayload = {
   message: {
    chatId: request.chatId,
    text: request.text,
    timestamp: request.timestamp || new Date().toISOString(),
    senderId: request.senderId,
    senderName: request.senderName,
   }
  };
  console.log('sendMessage payload:', messagePayload);
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
  if (error.response) {
   console.error('Ошибка при удалении чата:', error.message);
   console.error('Статус ответа:', error.response.status);
   console.error('Данные ответа:', error.response.data);
  } else {
   console.error('Ошибка при удалении чата:', error.message);
  }
  throw error;
 }
}
export async function removeUserFromChat(chatId, userId) {
    console.log(`Removing user ${userId} from chat ${chatId}...`);
    try {
        const response = await axios.delete('/Chat/RemoveUserFromChat', {
            withCredentials: true,
            params: {
                chatId: chatId,
                userId: userId,
            },
        });
        console.log('Удаление прошло успешно. Server response:', response.data);
        return response.data;
    } catch (error) {
        console.error('Ошибка при удалении пользователя из чата:', error);
        throw error;
    }
}

export async function setChatName(chatId, chatName) {
 try {
  const response = await axios.post('/Chat/SetChatName', null, {
   withCredentials: true,
   params: {
    chatId: chatId,
    chatName: chatName,
   },
  });
  return response.data;
 } catch (error) {
  console.error('Ошибка при изменении имени чата:', error);
  throw error;
 }
}

export async function getChatUsersList(chatId) {
 try {
  const response = await axios.get('/Chat/GetChatUsersList', {
   withCredentials: true,
   params: {
    chatId: chatId,
   },
  });
  return response.data;
 } catch (error) {
  console.error('Ошибка при получении списка участников чата:', error);
  throw error;
 }
}


