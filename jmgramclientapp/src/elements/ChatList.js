import React from 'react';
import ChatItem from './ChatItem';

function ChatList({ chats, isLoading, error, onChatNameChange, onChatClick, selectedChat }) {
  if (isLoading) {
    return <p>Загрузка списка чатов...</p>;
  }

  if (error) {
    return <p>Ошибка: {error}</p>;
  }

  return (
    <div className="chat-list">
      {chats.length > 0 ? (
        chats.map((chat) => (
          <ChatItem
            key={chat.ChatId || chat.id}
            chat={chat}
            onChatNameChange={onChatNameChange}
            onClick={() => onChatClick(chat)}
            isSelected={selectedChat && (selectedChat.id === chat.id || selectedChat.ChatId === chat.ChatId)}
          />
        ))
      ) : (
        <p>Чатов пока нет. Создайте новый!</p>
      )}
    </div>
  );
}

export default ChatList;
