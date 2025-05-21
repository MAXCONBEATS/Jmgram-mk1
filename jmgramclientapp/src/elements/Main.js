 import React, { useState, useEffect } from 'react';
 import '../css/Main.css';
 import logoutIcon from '../assets/images/logout_icon.png';
 import { getLastChatMessage, getUserChats } from '../controllers/ChatController';
 import CreateChatButton from './CreateChatButton'; // Импортируйте компонент кнопки
 import { getContactList } from '../controllers/ContactController';
 import UserSearch from './UserSearch';
 import axios from 'axios';
 axios.defaults.baseURL = 'https://localhost:5087';
 
 function Main({ userChats, error, onLogout }) {
  const [chatData, setChatData] = useState([]);
  const [userChatsState, setUserChatsState] = useState(userChats); // Добавляем локальное состояние для userChats
  const [contacts, setContacts] = useState([]); // Состояние для хранения контактов
  const [loading, setLoading] = useState(true); // Состояние для отслеживания загрузки
 
  const userId = localStorage.getItem('UserId'); // Получите UserId (пример)
 
  useEffect(() => {
   const fetchChatData = async () => {
    setLoading(true); // Начинаем загрузку
    if (userChatsState) {
     const chatDataPromises = userChatsState.map(async (chat) => {
      const lastChatMessage = await getLastChatMessage(chat.id);
      return {
       chatId: chat.id,
       chatName: chat.name,
       lastMessage: lastChatMessage ? lastChatMessage.text : 'Нет сообщений',
       lastMessageSender: lastChatMessage ? chat.lastMessageSender + ':' : '', // Используем SenderName
       lastMessageTime: lastChatMessage ? lastChatMessage.timestamp : null,
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
    console.log('Список контактов:', contactList); // Добавляем логирование
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
 
    <div className="main-content">
     <h2 className="contacts-header">Контакты:</h2>
     <div className="contacts-container">
        <UserSearch />
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
 
     <div className="chat-container">
      <h2>Чаты:</h2>
      <div className="chat-list">
       {loading ? (
        <p>Загрузка чатов...</p>
       ) : chatData.length > 0 ? (
        chatData.map((chat) => (
         <div key={chat.chatId} className="chat-item">
          <div className="chat-name">{chat.chatName}</div>
          <div className="last-message">
           <span className="sender">
            {chat.lastMessageSender ? chat.lastMessageSender + ':' : ''}
           </span>
           {chat.lastMessage} - <span className="time">{formatTime(chat.lastMessageTime)}</span>
          </div>
         </div>
        ))
       ) : (
        <p>Чатов пока нет. Создайте новый!</p> // Сообщение при отсутствии чатов
       )}
      </div>
      <CreateChatButton onCreateChat={handleCreateChat} /> {/* Добавляем кнопку */}
     </div>
    </div>
   </div>
  );
 }
 
 export default Main;