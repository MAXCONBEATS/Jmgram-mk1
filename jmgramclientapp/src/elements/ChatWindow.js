import React, { useState, useEffect } from 'react';
import { getMessages, sendMessage } from '../controllers/ChatController';
import '../css/ChatWindow.css';

function ChatWindow({ chat, onClose }) {
    const [messages, setMessages] = useState([]);
    const [newMessage, setNewMessage] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        async function fetchMessages() {
            if (!chat) {
                setMessages([]);
                return;
            }
            setLoading(true);
            try {
                const response = await getMessages(chat.chatId || chat.id, 1, 20);
                // Use senderName directly from message objects
                const mappedMessages = (response.chat || []).map(msg => ({
                    ...msg,
                    senderName: msg.senderName || 'Unknown'
                }));
                setMessages(mappedMessages);
            } catch (error) {
                console.error('Ошибка при загрузке сообщений:', error);
                setMessages([]);
            }
            setLoading(false);
        }
        fetchMessages();
    }, [chat]);

    const handleSendMessage = async () => {
        if (newMessage.trim() === '') {
            alert('Введите сообщение перед отправкой.');
            return;
        }
        try {
            await sendMessage({ chatId: chat.chatId || chat.id, text: newMessage });
            // Refresh messages after sending
            const response = await getMessages(chat.chatId || chat.id, 1, 20);
            const mappedMessages = (response.chat || []).map(msg => ({
                ...msg,
                senderName: msg.senderName || 'Unknown'
            }));
            setMessages(mappedMessages);
            setNewMessage('');
        } catch (error) {
            console.error('Ошибка при отправке сообщения:', error);
            alert('Ошибка при отправке сообщения.');
        }
    };

    if (!chat) {
        return <div className="chat-window">Выберите чат для просмотра сообщений</div>;
    }

    return (
        <div className="chat-window">
            <div className="chat-header">
                <h3>{chat.chatName || chat.name || 'Чат'}</h3>
                <button onClick={onClose}>Закрыть</button>
            </div>
            <div className="chat-messages-container">
                <div className="chat-messages">
                    {loading ? (
                        <p>Загрузка сообщений...</p>
                    ) : messages.length > 0 ? (
                        messages.map((msg) => (
                            <div key={msg.id || msg.messageId}>
                                <strong>{msg.senderName}:</strong> {msg.text}
                            </div>
                        ))
                    ) : (
                        <p>Сообщений пока нет.</p>
                    )}
                </div>
                <div className="chat-input-area">
                    <textarea
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        placeholder="Введите сообщение"
                        rows={3}
                    />
                    <button onClick={handleSendMessage}>Отправить</button>
                </div>
            </div>
        </div>
    );
}

export default ChatWindow;
