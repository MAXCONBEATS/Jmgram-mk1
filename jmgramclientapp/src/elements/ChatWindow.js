"use client"

import { useState, useCallback } from "react"
import { deleteMessage, getMessages } from "../controllers/ChatController"
import { useChatData } from "../hooks/useChatData"
import { useChatHub } from "../hooks/useChatHub"
import MessageList from "../components/MessageList"
import MessageInput from "../components/MessageInput"
import ParticipantsList from "../components/ParticipantsList"
import ProfileModal from "../elements/ProfileModal"
import "../css/ChatWindow.css"
import ChatAutoRefresh from "../components/ChatAutoRefresh"

function ChatWindow({ chat, onClose, currentUserId }) {
  const [editingMessageId, setEditingMessageId] = useState(null)
  const [replyingToMessage, setReplyingToMessage] = useState(null)
  const [profileUserId, setProfileUserId] = useState(null)

  const { messages, setMessages, participants, chatName, loading, loadingChatName, refreshParticipants } =
    useChatData(chat)

  // Функция для принудительного обновления сообщений (для новых файлов)
  const refreshMessages = useCallback(async () => {
    try {
      console.log("ChatWindow: принудительное обновление сообщений")
      const chatId = chat.chatId || chat.id
      console.log("ChatWindow: chatId =", chatId)

      const response = await getMessages(chatId, 1, 50) // Получаем последние 50 сообщений
      console.log("ChatWindow: полный ответ от getMessages:", response)
      console.log("ChatWindow: тип ответа:", typeof response)
      console.log("ChatWindow: является ли массивом:", Array.isArray(response))

      // Проверяем разные возможные структуры ответа
      let messagesData = null

      if (Array.isArray(response)) {
        // Если ответ - это массив сообщений
        messagesData = response
        console.log("ChatWindow: ответ - массив сообщений, длина:", messagesData.length)
      } else if (response && response.messages && Array.isArray(response.messages)) {
        // Если ответ - объект с полем messages
        messagesData = response.messages
        console.log("ChatWindow: ответ - объект с messages, длина:", messagesData.length)
      } else if (response && response.data && Array.isArray(response.data)) {
        // Если ответ - объект с полем data
        messagesData = response.data
        console.log("ChatWindow: ответ - объект с data, длина:", messagesData.length)
      } else if (response && response.chat && Array.isArray(response.chat)) {
        // Если ответ - объект с полем chat
        messagesData = response.chat
        console.log("ChatWindow: ответ - объект с chat, длина:", messagesData.length)
      } else if (response && Array.isArray(response.items)) {
        // Если ответ - объект с полем items (пагинация)
        messagesData = response.items
        console.log("ChatWindow: ответ - объект с items, длина:", messagesData.length)
      } else {
        console.error("ChatWindow: неожиданная структура ответа:", response)
        return
      }

      if (messagesData && messagesData.length >= 0) {
        console.log("ChatWindow: устанавливаем новые сообщения, количество:", messagesData.length)

        // Логируем первые несколько сообщений для проверки
        if (messagesData.length > 0) {
          console.log("ChatWindow: первое сообщение:", messagesData[0])
          if (messagesData.length > 1) {
            console.log("ChatWindow: последнее сообщение:", messagesData[messagesData.length - 1])
          }
        }

        // СОРТИРУЕМ ПО ВРЕМЕНИ: старые сообщения сначала, новые в конце
        const sortedMessages = [...messagesData].sort((a, b) => {
          // Сначала пробуем сортировать по timestamp
          const timestampA = new Date(a.timestamp || 0).getTime()
          const timestampB = new Date(b.timestamp || 0).getTime()

          if (timestampA !== timestampB) {
            return timestampA - timestampB // От старых к новым
          }

          // Если timestamp одинаковые, сортируем по ID
          const idA = Number.parseInt(a.id) || 0
          const idB = Number.parseInt(b.id) || 0
          return idA - idB // От меньших ID к большим
        })

        console.log("ChatWindow: после сортировки:")
        console.log("  Первое сообщение:", sortedMessages[0])
        console.log("  Последнее сообщение:", sortedMessages[sortedMessages.length - 1])

        setMessages(sortedMessages)
        console.log("ChatWindow: сообщения успешно обновлены")
      } else {
        console.warn("ChatWindow: messagesData пустой или undefined")
      }
    } catch (error) {
      console.error("ChatWindow: ошибка обновления сообщений", error)
      console.error("ChatWindow: детали ошибки:", {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
      })
    }
  }, [chat, setMessages])

  // Обработчики для ChatHub
  const handleMessageReceived = useCallback(
    (user, message, messageId, senderId, status) => {
      console.log("=== ПОЛУЧЕНО СООБЩЕНИЕ ===", { user, message, messageId, senderId, status })

      setMessages((prevMessages) => {
        // Проверяем, нет ли уже такого сообщения
        const existingMessage = prevMessages.find((msg) => msg.id === messageId)
        if (existingMessage) {
          console.log("Сообщение уже существует:", messageId)
          return prevMessages
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

        console.log("Добавляем новое сообщение:", newMessage)

        // Добавляем в конец (новые сообщения всегда последние)
        const updatedMessages = [...prevMessages, newMessage]

        console.log("Обновленный список сообщений:", updatedMessages.length)
        return updatedMessages
      })
    },
    [chat, setMessages],
  )

  const handleMessageUpdated = useCallback(
    (messageId, newText) => {
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
    },
    [editingMessageId, setMessages],
  )

  const handleMessageUpdateFailed = useCallback((messageId, error) => {
    alert("Ошибка при изменении сообщения: " + error)
  }, [])

  const { isConnected, sendMessage } = useChatHub(
    chat,
    handleMessageReceived,
    handleMessageUpdated,
    handleMessageUpdateFailed,
  )

  // Добавляем обработчик для успешной отправки сообщения
  const handleMessageSent = useCallback(() => {
    console.log("Сообщение отправлено, ожидаем получение через SignalR")

    // Добавляем небольшую задержку для обновления файлов
    setTimeout(() => {
      refreshMessages()
    }, 1000)
  }, [refreshMessages])

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
      {/* ДОБАВЛЯЕМ ChatAutoRefresh */}
      <ChatAutoRefresh onRefresh={refreshMessages} isEnabled={isConnected} interval={5000} />

      <div className="chat-header">
        <h3>
          {loadingChatName ? "Загрузка..." : chatName}
          <span className={`connection-status ${isConnected ? "connected" : "disconnected"}`}>
            {isConnected ? "🟢" : "🔴"}
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
