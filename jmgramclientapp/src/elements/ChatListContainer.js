import React, { useState, useEffect } from 'react';
import axios from 'axios';
import ChatList from './ChatList'; // Создадим этот компонент

function ChatListContainer() {
    const [chats, setChats] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const getChats = async () => {
            setIsLoading(true);
            setError(null);
            try {
                const response = await axios.get('/Chat/GetChatsForUser');
                console.log('Chats from server:', response.data); // Проверяем структуру данных
                setChats(response.data);
            } catch (error) {
                console.error('Ошибка при получении списка чатов:', error);
                setError('Не удалось загрузить список чатов.');
            } finally {
                setIsLoading(false);
            }
        };

        getChats();
    }, []);

    const handleChatNameChange = (chatId, newChatName) => {
        // Обновляем имя чата в списке
        setChats(chats.map(chat => chat.ChatId === chatId ? { ...chat, Name: newChatName } : chat)); // Используем chat.ChatId и обновляем Name
    };

    return (
        <ChatList chats={chats} isLoading={isLoading} error={error} onChatNameChange={handleChatNameChange} />
    );
}

export default ChatListContainer;