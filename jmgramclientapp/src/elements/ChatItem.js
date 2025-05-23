import axios from "axios";
import React, { useState } from 'react';

function ChatItem({ chat, onChatNameChange, onClick, isSelected }) {
    const [chatName, setChatName] = useState(chat.Name); // Используем chat.Name
    const [isEditing, setIsEditing] = useState(false);
    const [showDropdown, setShowDropdown] = useState(false);
    const [message, setMessage] = useState('');

    const handleChatNameChange = (event) => {
        setChatName(event.target.value);
    };

    const handleSaveChatName = async () => {
        try {
            await axios.post('/Chat/SetChatName', { chatId: chat.ChatId || chat.id, chatName: chatName }); // Используем chat.ChatId или chat.id
            onChatNameChange(chat.ChatId || chat.id, chatName); // Используем chat.ChatId или chat.id
            setIsEditing(false);
        } catch (error) {
            console.error('Ошибка при изменении имени чата:', error);
        }
    };

    const toggleDropdown = () => {
        setShowDropdown(!showDropdown);
    };

    const handleMessageChange = (event) => {
        setMessage(event.target.value);
    };

    const sendMessage = () => {
        if (message.trim() === '') {
            alert('Введите сообщение перед отправкой.');
            return;
        }
        // Placeholder for sending message logic
        console.log(`Sending message to chat ${chat.ChatId || chat.id}: ${message}`);
        // Clear message and close dropdown
        setMessage('');
        setShowDropdown(false);
    };

    return (
        <div
            onClick={onClick}
            style={{
                cursor: 'pointer',
                backgroundColor: 'transparent', // Remove background color change
                padding: '5px',
                marginBottom: '5px',
                borderRadius: '4px',
                border: isSelected ? '2px solid #444444' : '2px solid transparent', // Inner border for selected chat only with requested color
                boxSizing: 'border-box',
            }}
        >
            {isEditing ? (
                <>
                    <input type="text" value={chatName} onChange={handleChatNameChange} />
                    <button onClick={handleSaveChatName}>Сохранить</button>
                    <button onClick={() => setIsEditing(false)}>Отмена</button>
                </>
            ) : (
                <>
                    <div>
                        {chat.Name} {/* Используем chat.Name */}
                        <button onClick={(e) => { e.stopPropagation(); setIsEditing(true); }}>Изменить имя</button>
                    </div>
                    {showDropdown && (
                        <div style={{ marginTop: '8px', border: '1px solid #ccc', padding: '8px', borderRadius: '4px' }}>
                            <textarea
                                value={message}
                                onChange={handleMessageChange}
                                placeholder="Введите сообщение"
                                rows={3}
                                style={{ width: '100%' }}
                            />
                            <button onClick={sendMessage} style={{ marginTop: '4px' }}>Отправить</button>
                            <button onClick={() => setShowDropdown(false)} style={{ marginLeft: '8px', marginTop: '4px' }}>Отмена</button>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}
export default ChatItem;
