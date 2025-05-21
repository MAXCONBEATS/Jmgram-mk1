 import React, { useState } from 'react';
 import { createChat } from '../controllers/ChatController'; // Импортируем функцию createChat
 
 function CreateChatButton({ onCreateChat }) {
  const [chatName, setChatName] = useState('');
 
  const handleCreateChat = async () => {
   try {
    const newChat = await createChat({ name: chatName }); // Используем имя чата
    onCreateChat(newChat); // Сообщаем родителю о создании нового чата
    setChatName(''); // Очищаем поле ввода
   } catch (error) {
    console.error('Ошибка при создании чата:', error);
    // Обработайте ошибку (например, отобразите сообщение об ошибке)
   }
  };
 
  return (
   <div className="create-chat-container"> {/* Оборачиваем в div */}
    <input
     type="text"
     placeholder="Название чата"
     value={chatName}
     onChange={(e) => setChatName(e.target.value)}
    />
    <button onClick={handleCreateChat}>+ Создать чат</button>
   </div>
  );
 }
 
 export default CreateChatButton;