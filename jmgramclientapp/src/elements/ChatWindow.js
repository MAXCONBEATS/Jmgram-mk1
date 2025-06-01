import React, { useState, useEffect,useRef } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import EmojiPicker from 'emoji-picker-react';
import { getMessages, sendMessage, getChatUsersList, removeUserFromChat } from '../controllers/ChatController';
import '../css/ChatWindow.css';

function ChatWindow({ chat, onClose, senderId, senderName, currentUserId }) {
    const [messages, setMessages] = useState([]);
    const [newMessage, setNewMessage] = useState('');
    const [loading, setLoading] = useState(false);
    const [participants, setParticipants] = useState([]);
    const [contextMenuPosition, setContextMenuPosition] = useState({ x: 0, y: 0 });
    const [selectedParticipant, setSelectedParticipant] = useState(null);
    const emojiButtonRef = useRef(null);
    const [showEmojiPicker, setShowEmojiPicker] = useState(false);
    const chatRef = useRef(null);

    const [connection, setConnection] = useState(null);
    const [isConnected, setIsConnected] = useState(false);
    
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
        if (!chat) return;

const newConnection = new HubConnectionBuilder()
    .withUrl('https://localhost:5087/chatHub')
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

console.log('Creating new SignalR connection:', newConnection);

setConnection(newConnection);

return () => {
};
    }, [chat]);

    useEffect(() => {
        if (connection) {
            if (connection.state === "Disconnected") {
                connection.start()
                    .then(() => {
                        console.log('SignalR Connected.');
                        setIsConnected(true); // Set isConnected to true on successful connection

                        // Join the chat group
                        connection.invoke('JoinChat', chat.chatId || chat.id)
                            .then(() => console.log('Joined chat group:', chat.chatId || chat.id))
                            .catch(err => console.error('JoinChat error:', err));

                        // Listen for incoming messages
                        connection.on('ReceiveMessage', (user, message) => {
                            console.log('Received message from SignalR:', user, message);
                            setMessages(prevMessages => {
                                const newMessages = [...prevMessages, { senderName: user, text: message }];
                                console.log('Updated messages:', newMessages);
                                return newMessages;
                            });
                        });
                    })
                    .catch(e => console.error('Connection failed: ', e));
            } else {
                console.log('SignalR connection already started or connecting. Current state:', connection.state);
            }

            connection.onclose(error => {
                setIsConnected(false); // Set isConnected to false on connection close
                console.log("Соединение закрыто:", error); // Added log for connection close
                if (error) {
                    console.error('SignalR connection closed with error:', error);
                } else {
                    console.log('SignalR connection closed.');
                }
            });

            connection.onreconnecting(error => {
                setIsConnected(false); // Set isConnected to false on reconnecting
                console.warn('SignalR reconnecting due to error:', error);
            });

            connection.onreconnected(connectionId => {
                setIsConnected(true); // Set isConnected to true on reconnected
                console.log('SignalR reconnected. ConnectionId:', connectionId);
            });
        }

        return () => {
            if (connection && isConnected) { // Only leave chat if connected
                connection.off('ReceiveMessage');
                console.log('Attempting to leave chat:', chat.chatId || chat.id);
                connection.invoke('LeaveChat', chat.chatId || chat.id)
                    .then(() => console.log('Successfully left chat:', chat.chatId || chat.id))
                    .catch(err => console.error('LeaveChat error:', err));
                // Temporarily removed connection.stop() to check if error persists
                // connection.stop();
            }
        };
    }, [connection, chat, isConnected]);

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
            if (selectedParticipant && event.button === 0) { // left click only
                setSelectedParticipant(null);
            }
        };
        window.addEventListener('mousedown', handleClickOutside);
        return () => {
            window.removeEventListener('mousedown', handleClickOutside);
        };
    }, [selectedParticipant]);

    const handleContextMenu = (e, participant) => {
        e.preventDefault();
        console.log('Right click on participant:', participant);

        setContextMenuPosition({ x: e.clientX, y: e.clientY });
        setSelectedParticipant(participant); // Set selected participant
    };

    const handleRemoveUser = async (participantToRemove) => {
        console.log('Attempting to remove user:', participantToRemove);

        if (!participantToRemove) {
            console.warn('No participant selected for removal.');
            // setContextMenuVisible(false); // Removed since contextMenuVisible state is removed
            return;
        }

        console.log(`Removing user ${participantToRemove.id} from chat ${chat.chatId || chat.id}`);

        try {
            await removeUserFromChat(chat.chatId || chat.id, participantToRemove.id);
            console.log('User removed successfully');

            // Update participants state
            setParticipants(prevParticipants => prevParticipants.filter(p => p.id !== participantToRemove.id));
            // setContextMenuVisible(false); // Removed since contextMenuVisible state is removed

        } catch (error) {
            console.error('Error removing user:', error);
            // TODO: Show user the error
        }
    };


   
const handleSendMessage = async () => {
        if (newMessage.trim() === '') {
            alert('Введите сообщение перед отправкой.');
            return;
        }
        try {
            console.log('Sending message via SignalR:', newMessage);
            await connection.invoke('SendMessage', chat.chatId || chat.id, newMessage);
            setNewMessage('');
        } catch (error) {
            console.error('Ошибка при отправке сообщения через SignalR:', error);
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
  (() => {
    // Create a map from participant IDs to display names
    const participantNameMap = new Map();
    participants.forEach(p => {
      const displayName = p.firstName || p.phone || "Участник";
      participantNameMap.set(p.id, displayName);
    });
    return messages.map((message, index) => {
      // If senderName is an ID in participants, replace with display name
      const displayName = participantNameMap.get(message.senderName) || message.senderName || 'Unknown';
      return (
        <div key={index}>
          <strong>{displayName}:</strong> {message.text}
        </div>
      );
    });
  })()
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
                return (
                  <li key={participant.id} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <span>{participant.firstName || participant.phone || "Участник"}</span>
                    {currentUserId === chat.creatorUserId && participant.id !== chat.creatorUserId && (
                      <button
                        onClick={() => handleRemoveUser(participant)}
                        className="chat-header-button"
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

      {/* Removed global context menu rendering */}
    </div>
  </div>
);
}

export default ChatWindow;
