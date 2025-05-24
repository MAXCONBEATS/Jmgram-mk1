import React, { useState, useEffect,useRef } from 'react';
import EmojiPicker from 'emoji-picker-react';
import { getMessages, sendMessage, getChatUsersList, removeUserFromChat } from '../controllers/ChatController';
import '../css/ChatWindow.css';

function ChatWindow({ chat, onClose, senderId, senderName, currentUserId }) {
    const [messages, setMessages] = useState([]);
    const [newMessage, setNewMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [participants, setParticipants] = useState([]);
    const [contextMenuVisible, setContextMenuVisible] = useState(false);
    const [contextMenuPosition, setContextMenuPosition] = useState({ x: 0, y: 0 });
    const [selectedParticipant, setSelectedParticipant] = useState(null);
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
                // Filter duplicates by id before setting state
                const uniqueUsers = Array.from(new Map(users.map(u => [u.id, u])).values());
                setParticipants(uniqueUsers);
            } catch (error) {
                console.error('Ошибка при загрузке участников чата:', error);
                setParticipants([]);
            }
        }
        fetchParticipants();
    }, [chat]);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (contextMenuVisible && event.button === 0) { // left click only
                setContextMenuVisible(false);
                setSelectedParticipant(null);
            }
        };
        window.addEventListener('mousedown', handleClickOutside);
        return () => {
            window.removeEventListener('mousedown', handleClickOutside);
        };
    }, [contextMenuVisible]);

    const handleContextMenu = (event, participant) => {
        event.preventDefault();
        console.log('handleContextMenu called - currentUserId:', currentUserId, 'chat.creatorUserId:', chat.creatorUserId);
        // Temporarily allow context menu always for testing
        setSelectedParticipant(participant);
        setContextMenuPosition({ x: event.pageX, y: event.pageY });
        setContextMenuVisible(true);
        /*
        if (currentUserId === chat.creatorUserId) {
            setSelectedParticipant(participant);
            setContextMenuPosition({ x: event.pageX, y: event.pageY });
            setContextMenuVisible(true);
        }
        */
    };

    const handleRemoveUser = async (participant) => {
        console.log('Удаление пользователя...');
        if (!participant) {
            console.warn('Не выбран участник для удаления.');
            return;
        }
      console.log('participant id:', participant.id);
      console.log('chat id:', chat.chatId || chat.id);
  
        try {
            console.log(`User ${currentUserId} attempts to remove user ${participant.id} from chat ${chat.chatId || chat.id}`);
            const response = await removeUserFromChat(chat.chatId || chat.id, participant.id);
          
            console.log('Удаление успешно:', response);
            // Refresh participants list after successful removal
            const users = await getChatUsersList(chat.chatId || chat.id);
            const uniqueUsers = Array.from(new Map(users.map(u => [u.id, u])).values());
            setParticipants(uniqueUsers);
            setContextMenuVisible(false); // Закрываем контекстное меню
        } catch (error) {
            console.error('Ошибка при удалении пользователя:', error);
            // Show user the error message from backend if available
            if (error.response && error.response.data) {
                alert(`Ошибка: ${error.response.data}`);
            } else if (error.message) {
                alert(`Ошибка: ${error.message}`);
            } else {
                alert('Ошибка при удалении пользователя из чата.');
            }
        }
    };


   
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
              Array.from(new Map(participants.map(p => [p.id, p])).values()).map((participant) => {
                console.log('Rendering participant with id:', participant.id);
                return (
                  <li key={participant.id} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <span>{participant.firstName || participant.phone || "Участник"}</span>
                    {currentUserId === chat.creatorUserId && participant.id !== chat.creatorUserId && (
                      <button
                        onClick={() => handleRemoveUser(participant)}
                        style={{ marginLeft: '10px' }}
                      >
                        Удалить из чата
                      </button>
                    )}
                  </li>
                );
              })
            ) : (
              <li>Нет участников</li>
            )}
        </ul>
      </div>

      {contextMenuVisible && (
        // Temporarily removed context menu for removal
        null
      )}
    </div>
  </div>
);
}

export default ChatWindow;
