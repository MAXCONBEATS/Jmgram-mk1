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
    console.log("ChatController.createChat: отправляем запрос", request)
    console.log("ChatController.createChat: chat.chatType =", request.chat.chatType, typeof request.chat.chatType)

    const response = await axios.post("https://localhost:5087/Chat/Create", request, {
      withCredentials: true,
      headers: {
        "Content-Type": "application/json",
      },
    })

    console.log("ChatController.createChat: ответ получен", response.data)
    return response.data
  } catch (error) {
    console.error("ChatController.createChat: ошибка при создании чата:", error)
    console.error("Детали ошибки:", {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
    })
    throw error
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
    console.log("ChatController.getMessages: запрос сообщений", { chatId, pageNumber, pageSize })

    const response = await axios.get("https://localhost:5087/Chat/GetMessages", {
      withCredentials: true,
      params: {
        ChatId: chatId,
        PageNumber: pageNumber,
        PageSize: pageSize,
      },
    })

    console.log("ChatController.getMessages: ответ получен", {
      status: response.status,
      dataType: typeof response.data,
      isArray: Array.isArray(response.data),
      dataLength: Array.isArray(response.data) ? response.data.length : "не массив",
    })
    console.log("ChatController.getMessages: полные данные:", response.data)

    return response.data
  } catch (error) {
    console.error("ChatController.getMessages: ошибка при получении сообщений чата:", error)
    console.error("ChatController.getMessages: детали ошибки:", {
      message: error.message,
      status: error.response?.status,
      statusText: error.response?.statusText,
      url: error.config?.url,
      params: error.config?.params,
    })
    throw error
  }
}
export async function canSendMessage(chatId) {
  try {
    console.log("ChatController.canSendMessage: проверяем права для чата", chatId)

    // ИСПРАВЛЯЕМ URL: используем /Chat/ вместо /api/chat/
    const response = await axios.get(`https://localhost:5087/Chat/can-send-message/${chatId}`, {
      withCredentials: true,
      headers: {
        "Content-Type": "application/json",
      },
    })

    console.log("ChatController.canSendMessage: ответ получен", response.data)

    return {
      canSend: response.data.canSend,
      message: response.data.message,
      chatType: response.data.chatType,
    }
  } catch (error) {
    console.error("ChatController.canSendMessage: ошибка проверки прав:", error)

    // В случае ошибки возвращаем безопасное значение
    return {
      canSend: false,
      message: error.response?.data?.message || "Ошибка проверки прав",
      error: true,
    }
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
export async function deleteMessage(messageId) {
 try {
  const response = await axios.delete('https://localhost:5087/Chat/DeleteMessage', {
   withCredentials: true,
   params: {
    messageId: messageId,
   },
  });
  return response.data;
 } catch (error) {
   console.error('Ошибка при удалении чата:', error.message);
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
export async function getChatNameForUser(chatId) {
 try {
  const response = await axios.get('/Chat/GetChatNameForUser', {
   withCredentials: true,
   params: {
    chatId: chatId,
   },
  });
  return response.data;
 } catch (error) {
  console.error('Ошибка при получении имени чата:', error);
  throw error;
 }
}

export async function InviteToChat(chatId, senderUserId, recipientUserId) {
    try{
        const response = await axios.post('/Chat/InviteToChat', {
            chatId: chatId,
            senderId: senderUserId,
            recipientId: recipientUserId
        }, {
            withCredentials: true
        });
        return response.data;
    }
    catch(error){
        console.error('Ошибка при отправке приглашения в чат', error);
        throw error;
    }
}
export async function ResponseToInvite(chatInvitationId, accepted) {
    try{
        const response = await axios.post('/Chat/RespondToInvite',
            {
                chatInvitationId: chatInvitationId,
                accepted: accepted
            },
            {
                withCredentials: true,
                headers: {
                    'Content-Type': 'application/json'
                }
            }
        );
        return response.data;
    }
    catch(error){
        console.error('Ошибка при принятии приглашения в чат', error);
        throw error;
    }
}
export async function GetChatInvites() {
    try{
        const response = await axios.get('/Chat/ChatInvites', {
            withCredentials: true
        });
        return response.data.incomingRequests || [];
    }
    catch(error){
        console.error('Ошибка при получении списка приглашений в чат', error);
        throw error;
    }
}
export async function UpdateMessageText(messageId, text) {
    try{
        const response = await axios.patch('/Chat/UpdateMessageText', 
            {
                MessageId: messageId,
                Text: text
            },
            {
                withCredentials: true,
                headers: {
                    'Content-Type': 'application/json'
                }
            }
        );
        return response.data;
    }
    catch(error){
        console.error('Ошибка при изменении текста сообщения', error);
        throw error;
    }
}
