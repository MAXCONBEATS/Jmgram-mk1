import React, { useState, useEffect } from 'react';
import axios from 'axios';
import ChatList from './ChatList';
import { deleteChat } from '../controllers/ChatController';

function ChatListContainer({ selectedChat, setSelectedChat }) {
    const [chats, setChats] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const getChats = async () => {
            setIsLoading(true);
            setError(null);
            try {
                const response = await axios.get('/Chat/UserChats');
                let chatsData = response.data;

                const updatedChats = await Promise.all(chatsData.map(async (chat) => {
                    try {
                        const nameResponse = await axios.get('/Chat/GetChatNameForUser', {
                            params: { chatId: chat.id }
                        });
                        return { ...chat, Name: nameResponse.data };
                    } catch (error) {
                        console.error(`Ошибка при получении имени чата для чата ${chat.id}:`, error);
                        return chat;
                    }
                }));

                setChats(updatedChats);
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
        setChats(chats.map(chat => chat.id === chatId ? { ...chat, Name: newChatName } : chat));
    };

    const handleChatClick = (chat) => {
        setSelectedChat(chat);
    };

    const handleDeleteChat = async (chatId) => {
        try {
            await deleteChat(chatId);
            setChats((prevChats) => prevChats.filter(chat => chat.id !== chatId));
            if (selectedChat && (selectedChat.id === chatId || selectedChat.ChatId === chatId)) {
                // If the deleted chat is currently open, close it and select another chat if available
                setSelectedChat(null);
                // Optionally, select the first chat in the updated list if any
                setTimeout(() => {
                    setChats((prevChats) => {
                        if (prevChats.length > 0) {
                            setSelectedChat(prevChats[0]);
                        }
                        return prevChats;
                    });
                }, 0);
            }
        } catch (error) {
            console.error('Ошибка при удалении чата:', error);
        }
    };

    return (
        <ChatList
            chats={chats}
            isLoading={isLoading}
            error={error}
            onChatNameChange={handleChatNameChange}
            onChatClick={handleChatClick}
            onDeleteChat={handleDeleteChat}
            selectedChat={selectedChat}
        />
    );
}

export default ChatListContainer;
