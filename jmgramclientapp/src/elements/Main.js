import React, { useState, useEffect } from 'react';
import '../css/Main.css';
import logoutIcon from '../assets/images/logout_icon.png';
import { getLastChatMessage } from '../controllers/ChatController';

function Main({ userChats, contacts, error, onLogout }) {
 const [chatData, setChatData] = useState([]);

 useEffect(() => {
  const fetchChatData = async () => {
   if (userChats) {
    const chatDataPromises = userChats.map(async (chat) => {
     const lastChatMessage = await getLastChatMessage(chat.id);
     return {
      chatId: chat.id,
      chatName: chat.name,
      lastMessage: lastChatMessage ? lastChatMessage.text : 'Нет сообщений',
      lastMessageSender: lastChatMessage ? lastChatMessage.senderName : '', // Используем SenderName
      lastMessageTime: lastChatMessage ? lastChatMessage.timestamp : null,
     };
    });

    const chatData = await Promise.all(chatDataPromises);
    setChatData(chatData);
   }
  };

  fetchChatData();
 }, [userChats]);

 const formatTime = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
 };

 return (
  <div className="main-container">
   <img src={logoutIcon} alt="Выйти" className="logout-icon" onClick={onLogout} />

   {error && <p className="error-message">{error}</p>}

   <h2>Чаты:</h2>
   <div className="chat-list">
    {chatData.length > 0 ? (
     chatData.map((chat) => (
      <div key={chat.chatId} className="chat-item">
       <div className="chat-name">{chat.chatName}</div>
       <div className="last-message">
        <span className="sender">
         {chat.lastMessageSender ? chat.lastMessageSender + ':' : ''} {/* Отображаем имя отправителя */}
        </span>
        {chat.lastMessage} - <span className="time">{formatTime(chat.lastMessageTime)}</span>
       </div>
      </div>
     ))
    ) : (
     <p>Загрузка чатов...</p>
    )}
   </div>

   <h2>Контакты:</h2>
   <ul className="contact-list">
    {contacts.map((contact, index) => (
     <li key={index}>{contact}</li>
    ))}
   </ul>
  </div>
 );
}

export default Main;