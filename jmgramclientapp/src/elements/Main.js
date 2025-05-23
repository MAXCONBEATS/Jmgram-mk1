import React, { useState, useEffect } from 'react';
import '../css/Main.css';
import logoutIcon from '../assets/images/logout_icon.png';
import { getLastChatMessage, getUserChats } from '../controllers/ChatController';
import CreateChatButton from './CreateChatButton'; // Импортируйте компонент кнопки
import { getContactList } from '../controllers/ContactController';
import UserSearch from './UserSearch';
import ChatWindow from './ChatWindow';
import axios from 'axios';
axios.defaults.baseURL = 'https://localhost:5087';

 function Main({ userChats, error, onLogout }) {
 
  const [chatData, setChatData] = useState([]);
  const [userChatsState, setUserChatsState] = useState(userChats); // Добавляем локальное состояние для userChats
  const [contacts, setContacts] = useState([]); // Состояние для хранения контактов
  const [loading, setLoading] = useState(true); // Состояние для отслеживания загрузки
  const [selectedChat, setSelectedChat] = useState(null); // Новое состояние для выбранного чата
 
  const userId = localStorage.getItem('UserId'); // Получите UserId (пример)

  useEffect(() => {
    setUserChatsState(userChats);
  }, [userChats]);

 // Helper function to get the other user's name in a chat
 const getOtherUserName = (chat) => {
   if (!chat.chatUsers || chat.chatUsers.length === 0) return '';
   // Assuming chat.chatUsers is an array of user objects with id and name
   const otherUser = chat.chatUsers.find(user => user.id !== userId);
   return otherUser ? otherUser.name : '';
 };

 // Format chat name as "Переписка с {имя другого пользователя}"
 const formatChatName = (chat) => {
   const otherUserName = getOtherUserName(chat);
   return otherUserName ? `Переписка с ${otherUserName}` : chat.name;
 };

 console.log('userChatsState:', userChatsState);
 console.log('chatData:', chatData);

 useEffect(() => {
  const fetchChatData = async () => {
   setLoading(true); // Начинаем загрузку
   if (userChatsState) {
     const chatDataPromises = userChatsState.map(async (chat) => {
      const lastChatMessage = await getLastChatMessage(chat.id);
      return {
       chatId: chat.id,
       chatName: formatChatName(chat),
       lastMessage: lastChatMessage ? lastChatMessage.text : 'Нет сообщений',
       lastMessageSender: lastChatMessage ? lastChatMessage.senderName + ':' : '', // Используем senderName
       lastMessageTime: lastChatMessage ? lastChatMessage.timestamp : null,
       chatUsers: chat.chatUsers, // Keep chatUsers for name formatting
      };
     });

    const chatData = await Promise.all(chatDataPromises);
    setChatData(chatData);
   }
   setLoading(false); // Загрузка завершена
  };

  fetchChatData();
 }, [userChatsState]);

 useEffect(() => {
 const fetchContacts = async () => {
  try {
   const contactList = await getContactList(); // Убрали передачу userId
   setContacts(contactList); // Сохраняем полученные контакты в состоянии
  } catch (error) {
   console.error('Ошибка при получении списка контактов:', error);
   // Обработайте ошибку (например, отобразите сообщение об ошибке)
  }
 };

 fetchContacts();
}, []);

 const formatTime = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
 };

 const handleCreateChat = (newChat) => {
  // Обновляем локальное состояние userChats с новым чатом
  setUserChatsState([...userChatsState, newChat]);
 };

 const handleAcceptContactRequest = async (contactRequestId) => {
  try {
   const response = await axios.post('http://localhost:5087/Contact/Accept', { contactRequestId }, { withCredentials: true });
   if (response.status === 200) {
    console.log('Запрос на добавление в друзья принят.');
    // Обновляем список чатов
    const updatedUserChats = await getUserChats();
    setUserChatsState(updatedUserChats);
   } else {
    console.error('Ошибка при принятии запроса в друзья:', response.status);
    // Обработайте ошибку
   }
  } catch (error) {
   console.error('Ошибка при принятии запроса в друзья:', error);
   // Обработайте ошибку
  }
 };

 return (
  <div className="main-container">
   <img src={logoutIcon} alt="Выйти" className="logout-icon" onClick={onLogout} />

   {error && <p className="error-message">{error}</p>}

    <div className="main-content" style={{ display: 'flex', gap: '20px' }}>
     <div style={{ flex: '1', maxWidth: '200px' }}>
      <h2 className="contacts-header">Контакты:</h2>
      <div className="user-search-container">
       <UserSearch />
      </div>

      <div className="contacts-container">
       <ul className="contact-list">
        {contacts.length > 0 ? (
         contacts.map((contact) => (
          <li key={contact.contactUserId}>{contact.name}</li>
         ))
        ) : (
         <li>У вас пока нет контактов</li> // Сообщение при отсутствии контактов
        )}
       </ul>
      </div>
     </div>

     <div style={{ flex: '2' }}>
      <h2>Чаты:</h2>
      <div className="chat-list">
       {loading ? (
        <p>Загрузка чатов...</p>
       ) : chatData.length > 0 ? (
        chatData.map((chat) => (
         <div
          key={chat.chatId}
          className={`chat-item${selectedChat && selectedChat.chatId === chat.chatId ? ' selected' : ''}`}
          onClick={() => setSelectedChat(chat)}
          style={{ cursor: 'pointer', padding: '5px', borderBottom: '1px solid #ccc' }}
         >
          <div className="chat-name">{chat.chatName}</div>
          <div className="last-message" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
           <div>
            <strong>{chat.lastMessageSender}</strong> {chat.lastMessage}
           </div>
           <div style={{ marginLeft: '10px', whiteSpace: 'nowrap' }}>
            {formatTime(chat.lastMessageTime)}
           </div>
          </div>
         </div>
        ))
       ) : (
        <p>Чатов пока нет. Создайте новый!</p> // Сообщение при отсутствии чатов
       )}
      </div>
      <div className="create-chat-button-wrapper">
       <CreateChatButton contacts={contacts} onCreateChat={handleCreateChat} /> {/* Добавляем кнопку */}
      </div>
     </div>

     <div style={{ flex: '3', display: 'flex', justifyContent: 'center' }}>
      <ChatWindow chat={selectedChat} onClose={() => setSelectedChat(null)} />
     </div>
    </div>
  </div>
 );
}

export default Main;
