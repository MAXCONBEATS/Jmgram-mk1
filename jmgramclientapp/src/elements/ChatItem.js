import React, { useState, useEffect, useRef } from 'react';
import { setChatName } from '../controllers/ChatController';

function ChatItem({ chat, onChatNameChange, onClick, isSelected, onDeleteChat }) {
    const [chatName, setChatNameState] = useState(chat.Name);
    const [isEditing, setIsEditing] = useState(false);
    const [showDropdown, setShowDropdown] = useState(false);
    const [message, setMessage] = useState('');
    const [contextMenuVisible, setContextMenuVisible] = useState(false);
    const [contextMenuPosition, setContextMenuPosition] = useState({ x: 0, y: 0 });
    const contextMenuRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (contextMenuRef.current && !contextMenuRef.current.contains(event.target)) {
                setContextMenuVisible(false);
            }
        };
        if (contextMenuVisible) {
            document.addEventListener('click', handleClickOutside);
        } else {
            document.removeEventListener('click', handleClickOutside);
        }
        return () => {
            document.removeEventListener('click', handleClickOutside);
        };
    }, [contextMenuVisible]);

    const handleChatNameChange = (event) => {
        setChatNameState(event.target.value);
    };

    const handleSaveChatName = async () => {
        try {
            await setChatName(chat.ChatId || chat.id, chatName);
            onChatNameChange(chat.ChatId || chat.id, chatName);
            setIsEditing(false);
            setContextMenuVisible(false);
        } catch (error) {
            console.error('Ошибка при изменении имени чата:', error);
        }
    };

    const handleContextMenu = (event) => {
        event.preventDefault();
        setContextMenuPosition({ x: event.clientX, y: event.clientY });
        setContextMenuVisible(true);
    };

    const handleDeleteChat = async () => {
        try {
            await onDeleteChat(chat.ChatId || chat.id);
            setContextMenuVisible(false);
        } catch (error) {
            console.error('Ошибка при удалении чата:', error);
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
        console.log(`Sending message to chat ${chat.ChatId || chat.id}: ${message}`);
        setMessage('');
        setShowDropdown(false);
    };

    return (
        <div
            onClick={onClick}
            onContextMenu={handleContextMenu}
            style={{
                cursor: 'pointer',
                backgroundColor: 'transparent',
                padding: '5px',
                marginBottom: '5px',
                borderRadius: '4px',
                border: isSelected ? '2px solid #444444' : '2px solid transparent',
                boxSizing: 'border-box',
                position: 'relative',
            }}
        >
                {isEditing ? (
                    <>
                        <input type="text" value={chatName} onChange={handleChatNameChange} onClick={(event) => event.stopPropagation()} />
                        <button onClick={(event) => { event.stopPropagation(); handleSaveChatName(); }}>Сохранить</button>
                        <button onClick={(event) => { event.stopPropagation(); setIsEditing(false); }}>Отмена</button>
                    </>
                ) : (
                <>
                    <div>
                        {chat.Name}
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
            {contextMenuVisible && (
                <ul
                    ref={contextMenuRef}
                    className="context-menu"
                    style={{
                        top: contextMenuPosition.y,
                        left: contextMenuPosition.x,
                        position: 'fixed',
                        zIndex: 1000,
                        minWidth: '150px',
                    }}
                >
                    <li
                        className="context-menu-item"
                        onClick={(event) => {
                            event.stopPropagation();
                            setIsEditing(true);
                            setContextMenuVisible(false);
                        }}
                    >
                        Изменить имя
                    </li>
                    <li
                        className="context-menu-item"
                        onClick={(event) => {
                            event.stopPropagation();
                            handleDeleteChat();
                        }}
                    >
                        Удалить чат
                    </li>
                </ul>
            )}
        </div>
    );
}

export default ChatItem;
