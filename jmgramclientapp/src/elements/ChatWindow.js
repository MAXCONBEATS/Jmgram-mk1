import React, { useState, useEffect,useRef } from 'react';
import EmojiPicker from 'emoji-picker-react';
import { getMessages, sendMessage, getChatUsersList } from '../controllers/ChatController';
import '../css/ChatWindow.css';

function ChatWindow({ chat, onClose, senderId, senderName }) {
    const [messages, setMessages] = useState([]);
    const [newMessage, setNewMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [participants, setParticipants] = useState([]);
    const emojiButtonRef = useRef(null);
    const [showEmojiPicker, setShowEmojiPicker] = useState(false);
    const chatRef = useRef(null);
    
    const handleEmojiClick = (emojiData) => {
    setNewMessage(prev => prev + emojiData.emoji);
    setShowEmojiPicker(false);
  };


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

    useEffect(() => {
        async function fetchParticipants() {
            if (!chat) {
                setParticipants([]);
                return;
            }
            try {
                const users = await getChatUsersList(chat.chatId || chat.id);
                setParticipants(users);
            } catch (error) {
                console.error('Ошибка при загрузке участников чата:', error);
                setParticipants([]);
            }
        }
        fetchParticipants();
    }, [chat]);


   
const handleSendMessage = async () => {
        if (newMessage.trim() === '') {
            alert('Введите сообщение перед отправкой.');
            return;
        }
        try {
            const messagePayload = { chatId: chat.chatId || chat.id, text: newMessage, senderId: senderId, senderName: senderName };
            console.log('Sending message payload:', messagePayload);
            await sendMessage(messagePayload);
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
    if (!chat) return null;

    return (
  <div className="chat-window">
    {/* Шапка чата */}
    <div className="chat-header">
      <h3>{chat?.name || chat?.chatName || chat?.chatName || "Чат"}</h3>
      <button onClick={onClose}>Закрыть</button>
    </div>

    {/* Основное содержимое: сообщения + участники */}
    <div className="chat-content">
      {/* Левый блок: сообщения */}
      <div className="chat-messages-section">
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

        {/* Поле ввода (остаётся внизу ЛЕВОЙ колонки) */}
        <div className="chat-input-area">
          <textarea
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            placeholder="Введите сообщение"
            rows={3}
          />
          <button 
        ref={emojiButtonRef}
        className="emoji-trigger"
        onClick={() => setShowEmojiPicker(!showEmojiPicker)}
      >
        😊
      </button>
      <button onClick={handleSendMessage}>Отправить</button>
      {showEmojiPicker && (
        <div className="emoji-picker-wrapper">
          <EmojiPicker
            onEmojiClick={(emojiData) => {
              setNewMessage(prev => prev + emojiData.emoji);
              setShowEmojiPicker(false);
            }}
            width={300}
            height={350}
            previewConfig={{ showPreview: false }}
            skinTone={false}
            searchDisabled={false}
            theme="dark"
          />
        </div>
      )}
        </div>
      </div>

      {/* Правый блок: участники */}
      <div className="chat-participants-column">
        <h4>Участники</h4>
        <ul>
          {participants.length > 0 ? (
            participants.map((participant) => (
              <li key={participant.id}>
                {participant.firstName || participant.phone || "Участник"}
              </li>
            ))
          ) : (
            <li>Нет участников</li>
          )}
        </ul>
      </div>
    </div>
  </div>
);
}

export default ChatWindow;
