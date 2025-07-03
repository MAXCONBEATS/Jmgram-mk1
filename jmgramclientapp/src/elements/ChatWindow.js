import { useState, useCallback } from "react"
import { deleteMessage } from "../controllers/ChatController"
import { useChatData } from "../hooks/useChatData"
import { useChatHub } from "../hooks/useChatHub"
import MessageList from "../components/MessageList"
import MessageInput from "../components/MessageInput"
import ParticipantsList from "../components/ParticipantsList"
import ProfileModal from "../elements/ProfileModal"
import "../css/ChatWindow.css"

function ChatWindow({ chat, onClose, currentUserId }) {
  const [editingMessageId, setEditingMessageId] = useState(null)
  const [replyingToMessage, setReplyingToMessage] = useState(null)
  const [profileUserId, setProfileUserId] = useState(null)

  const {
    messages,
    setMessages,
    participants,
    chatName,
    loading,
    loadingChatName,
    refreshParticipants
  } = useChatData(chat)

  // Обработчики для ChatHub
  const handleMessageReceived = useCallback((user, message, messageId, senderId, status) => {
    console.log("=== ПОЛУЧЕНО СООБЩЕНИЕ ===", { user, message, messageId, senderId, status });
    
    setMessages((prevMessages) => {
      // Проверяем, нет ли уже такого сообщения
      const existingMessage = prevMessages.find(msg => msg.id === messageId);
      if (existingMessage) {
        console.log("Сообщение уже существует:", messageId);
        return prevMessages;
      }

      const newMessage = {
        id: messageId || Date.now().toString(),
        senderName: user,
        senderId: senderId,
        text: message,
        status: status || "Sent",
        timestamp: new Date().toISOString(),
        chatId: chat.chatId || chat.id,
      }
      
      console.log("Добавляем новое сообщение:", newMessage);
      const updatedMessages = [...prevMessages, newMessage]
      
      const sortedMessages = updatedMessages.sort((a, b) => {
        if (!isNaN(a.id) && !isNaN(b.id)) {
          return parseInt(a.id) - parseInt(b.id)
        }
        const dateA = new Date(a.timestamp || 0)
        const dateB = new Date(b.timestamp || 0)
        return dateA - dateB
      })
      
      console.log("Обновленный список сообщений:", sortedMessages.length);
      return sortedMessages
    })
  }, [chat, setMessages])

  const handleMessageUpdated = useCallback((messageId, newText) => {
    setMessages((prevMessages) =>
      prevMessages.map((msg) => {
        if (msg.id === messageId) {
          return { ...msg, text: newText }
        }
        return msg
      }),
    )
    if (editingMessageId === messageId) {
      setEditingMessageId(null)
    }
  }, [editingMessageId, setMessages])

  const handleMessageUpdateFailed = useCallback((messageId, error) => {
    alert("Ошибка при изменении сообщения: " + error)
  }, [])

  const { isConnected, sendMessage } = useChatHub(chat, handleMessageReceived, handleMessageUpdated, handleMessageUpdateFailed)

  // Добавляем обработчик для успешной отправки сообщения
  const handleMessageSent = useCallback(() => {
    console.log("Сообщение отправлено, ожидаем получение через SignalR");
  }, []);

  const handleDeleteMessage = async (message) => {
    const confirmDelete = window.confirm(
      `Вы уверены, что хотите удалить это сообщение?\n\n"${message.text.substring(0, 100)}${message.text.length > 100 ? "..." : ""}"`,
    )
    if (!confirmDelete) return

    try {
      await deleteMessage(message.id)
      setMessages((prevMessages) => prevMessages.filter((msg) => msg.id !== message.id))
      if (editingMessageId === message.id) {
        setEditingMessageId(null)
      }
      if (replyingToMessage && replyingToMessage.id === message.id) {
        setReplyingToMessage(null)
      }
    } catch (error) {
      console.error("Ошибка при удалении сообщения:", error)
      alert("Ошибка при удалении сообщения. Попробуйте еще раз.")
    }
  }

  const startEditingMessage = (message) => {
    setEditingMessageId(message.id)
  }

  const startReplyToMessage = (message) => {
    setReplyingToMessage(message)
  }

  if (!chat) return null

  const participantNameMap = new Map()
  participants.forEach((p) => {
    const displayName = p.firstName || p.phone || "Участник"
    participantNameMap.set(p.id, displayName)
  })

  return (
    <div className="chat-window">
      <div className="chat-header">
        <h3>
          {loadingChatName ? "Загрузка..." : chatName}
          <span className={`connection-status ${isConnected ? 'connected' : 'disconnected'}`}>
            {isConnected ? '🟢' : '🔴'}
          </span>
        </h3>
        <button onClick={onClose}>Закрыть</button>
      </div>
      <div className="chat-content">
        <div className="chat-messages-section">
          <div className="chat-messages">
            <MessageList
              messages={messages}
              loading={loading}
              participantNameMap={participantNameMap}
              currentUserId={currentUserId}
              editingMessageId={editingMessageId}
              onStartEditing={startEditingMessage}
              onStartReply={startReplyToMessage}
              onDeleteMessage={handleDeleteMessage}
            />
          </div>
          <MessageInput
            chat={chat}
            isConnected={isConnected}
            sendMessage={sendMessage}
            editingMessageId={editingMessageId}
            replyingToMessage={replyingToMessage}
            participantNameMap={participantNameMap}
            onCancelReply={() => setReplyingToMessage(null)}
            onEditingComplete={() => setEditingMessageId(null)}
            onMessageSent={handleMessageSent}
          />
        </div>
        <ParticipantsList
          participants={participants}
          chat={chat}
          currentUserId={currentUserId}
          onParticipantsUpdate={refreshParticipants}
          onOpenProfile={setProfileUserId}
        />
      </div>
      {profileUserId && (
        <ProfileModal
          userId={profileUserId}
          onClose={() => setProfileUserId(null)}
          onContactDeleted={() => {
            refreshParticipants()
            setProfileUserId(null)
          }}
        />
      )}
    </div>
  )
}

export default ChatWindow