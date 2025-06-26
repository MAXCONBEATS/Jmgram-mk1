import { useState, useEffect, useRef } from "react"
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr"
import EmojiPicker from "emoji-picker-react"
import {
  getMessages,
  getChatUsersList,
  removeUserFromChat,
  InviteToChat,
  deleteMessage,
} from "../controllers/ChatController"
import { getContactList } from "../controllers/ContactController"
import "../css/ChatWindow.css"
import ProfileModal from "../elements/ProfileModal"

// Функция для форматирования времени с добавлением 5 часов
const formatMessageTime = (timestamp) => {
  if (!timestamp) return ""

  const date = new Date(timestamp)
  // Добавляем 5 часов (5 * 60 * 60 * 1000 миллисекунд)
  const correctedDate = new Date(date.getTime() + 5 * 60 * 60 * 1000)

  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const messageDate = new Date(correctedDate.getFullYear(), correctedDate.getMonth(), correctedDate.getDate())

  const timeString = correctedDate.toLocaleTimeString("ru-RU", {
    hour: "2-digit",
    minute: "2-digit",
  })

  // Если сообщение сегодня - показываем только время
  if (messageDate.getTime() === today.getTime()) {
    return timeString
  }

  // Если вчера
  const yesterday = new Date(today.getTime() - 24 * 60 * 60 * 1000)
  if (messageDate.getTime() === yesterday.getTime()) {
    return `вчера ${timeString}`
  }

  // Если в этом году - показываем дату без года
  if (correctedDate.getFullYear() === now.getFullYear()) {
    return (
      correctedDate.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "2-digit",
      }) + ` ${timeString}`
    )
  }

  // Полная дата
  return (
    correctedDate.toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    }) + ` ${timeString}`
  )
}

function ChatWindow({ chat, onClose, senderId, senderName, currentUserId }) {
  const [messages, setMessages] = useState([])
  const [newMessage, setNewMessage] = useState("")
  const [editingMessageId, setEditingMessageId] = useState(null)
  const [loading, setLoading] = useState(false)
  const [participants, setParticipants] = useState([])
  const [contextMenuPosition, setContextMenuPosition] = useState({ x: 0, y: 0 })
  const [selectedParticipant, setSelectedParticipant] = useState(null)
  const emojiButtonRef = useRef(null)
  const [showEmojiPicker, setShowEmojiPicker] = useState(false)
  const chatRef = useRef(null)

  const [connection, setConnection] = useState(null)
  const [isConnected, setIsConnected] = useState(false)

  const [showInviteModal, setShowInviteModal] = useState(false)
  const [availableContacts, setAvailableContacts] = useState([])
  const [loadingContacts, setLoadingContacts] = useState(false)

  const [replyingToMessage, setReplyingToMessage] = useState(null)

  const [profileUserId, setProfileUserId] = useState(null)

  useEffect(() => {
    async function fetchMessages() {
      if (!chat) {
        setMessages([])
        return
      }
      setLoading(true)
      try {
        const response = await getMessages(chat.chatId || chat.id, 1, 20)
        const mappedMessages = (response.chat || []).map((msg) => ({
          ...msg,
          senderName: msg.senderName || "Unknown",
        }))
        setMessages(mappedMessages)
      } catch (error) {
        console.error("Ошибка при загрузке сообщений:", error)
        setMessages([])
      }
      setLoading(false)
    }
    fetchMessages()
  }, [chat])

  useEffect(() => {
    if (!chat) return

    const newConnection = new HubConnectionBuilder()
      .withUrl("https://localhost:5087/chatHub")
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build()

    console.log("Creating new SignalR connection:", newConnection)

    setConnection(newConnection)

    return () => {
      // Cleanup if needed
    }
  }, [chat])

  useEffect(() => {
    if (connection) {
      if (connection.state === "Disconnected") {
        connection
          .start()
          .then(() => {
            console.log("SignalR Connected.")
            setIsConnected(true)

            connection
              .invoke("JoinChat", chat.chatId || chat.id)
              .then(() => console.log("Joined chat group:", chat.chatId || chat.id))
              .catch((err) => console.error("JoinChat error:", err))

            connection.on("ReceiveMessage", (user, message, messageId, senderId, status) => {
              console.log("Received message from SignalR:", user, message, "Status:", status, "SenderId:", senderId)
              setMessages((prevMessages) => {
                const newMessage = {
                  id: messageId || Date.now().toString(),
                  senderName: user,
                  senderId: senderId,
                  text: message,
                  status: status || "Sent",
                  timestamp: new Date().toISOString(), // Текущее время для новых сообщений
                  chatId: chat.chatId || chat.id,
                }
                const newMessages = [...prevMessages, newMessage]
                console.log("Updated messages with status:", newMessages)
                return newMessages
              })
            })

            connection.on("MessageUpdated", (messageId, newText, userId) => {
              console.log("Message updated via SignalR:", messageId, newText)
              setMessages((prevMessages) =>
                prevMessages.map((msg) => {
                  if (msg.id === messageId) {
                    return { ...msg, text: newText }
                  }
                  return msg
                }),
              )
              // Сбрасываем режим редактирования если это наше сообщение
              if (editingMessageId === messageId) {
                setEditingMessageId(null)
                setNewMessage("")
              }
            })

            connection.on("MessageUpdateFailed", (messageId, error) => {
              console.error("Message update failed:", error)
              alert("Ошибка при изменении сообщения: " + error)
            })
          })
          .catch((e) => console.error("Connection failed: ", e))
      } else {
        console.log("SignalR connection already started or connecting. Current state:", connection.state)
      }

      connection.onclose((error) => {
        setIsConnected(false)
        console.log("Соединение закрыто:", error)
        if (error) {
          console.error("SignalR connection closed with error:", error)
        } else {
          console.log("SignalR connection closed.")
        }
      })

      connection.onreconnecting((error) => {
        setIsConnected(false)
        console.warn("SignalR reconnecting due to error:", error)
      })

      connection.onreconnected((connectionId) => {
        setIsConnected(true)
        console.log("SignalR reconnected. ConnectionId:", connectionId)
      })
    }

    return () => {
      if (connection && isConnected) {
        connection.off("ReceiveMessage")
        connection.off("MessageUpdated")
        connection.off("MessageUpdateFailed")
        console.log("Attempting to leave chat:", chat.chatId || chat.id)
        connection
          .invoke("LeaveChat", chat.chatId || chat.id)
          .then(() => console.log("Successfully left chat:", chat.chatId || chat.id))
          .catch((err) => console.error("LeaveChat error:", err))
      }
    }
  }, [connection, chat, isConnected])

  useEffect(() => {
    async function fetchParticipants() {
      if (!chat) {
        setParticipants([])
        return
      }
      try {
        const users = await getChatUsersList(chat.chatId || chat.id)
        const uniqueUsers = Array.from(new Map(users.map((u) => [u.id, u])).values())
        setParticipants(uniqueUsers)
      } catch (error) {
        console.error("Ошибка при загрузке участников чата:", error)
        setParticipants([])
      }
    }
    fetchParticipants()
  }, [chat])

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (selectedParticipant && event.button === 0) {
        setSelectedParticipant(null)
      }
    }
    window.addEventListener("mousedown", handleClickOutside)
    return () => {
      window.removeEventListener("mousedown", handleClickOutside)
    }
  }, [selectedParticipant])

  const handleContextMenu = (e, participant) => {
    e.preventDefault()
    console.log("Right click on participant:", participant)

    setContextMenuPosition({ x: e.clientX, y: e.clientY })
    setSelectedParticipant(participant)
  }

  const handleRemoveUser = async (participantToRemove) => {
    console.log("Attempting to remove user:", participantToRemove)

    if (!participantToRemove) {
      console.warn("No participant selected for removal.")
      return
    }

    console.log(`Removing user ${participantToRemove.id} from chat ${chat.chatId || chat.id}`)

    try {
      await removeUserFromChat(chat.chatId || chat.id, participantToRemove.id)
      console.log("User removed successfully")

      setParticipants((prevParticipants) => prevParticipants.filter((p) => p.id !== participantToRemove.id))
    } catch (error) {
      console.error("Error removing user:", error)
    }
  }

  const loadAvailableContacts = async () => {
    setLoadingContacts(true)
    try {
      const allContacts = await getContactList()
      // Фильтруем контакты, исключая уже участвующих в чате
      const participantIds = participants.map((p) => p.id)
      const availableForInvite = allContacts.filter((contact) => !participantIds.includes(contact.id))
      setAvailableContacts(availableForInvite)
    } catch (error) {
      console.error("Ошибка при загрузке контактов:", error)
      alert("Ошибка при загрузке контактов")
    }
    setLoadingContacts(false)
  }

  const handleInviteToChat = async (contactId) => {
    try {
      await InviteToChat(chat.chatId || chat.id, currentUserId, contactId)
      alert("Приглашение отправлено!")
      setShowInviteModal(false)
      // Обновляем список участников
      const users = await getChatUsersList(chat.chatId || chat.id)
      const uniqueUsers = Array.from(new Map(users.map((u) => [u.id, u])).values())
      setParticipants(uniqueUsers)
    } catch (error) {
      console.error("Ошибка при отправке приглашения:", error)
      alert("Ошибка при отправке приглашения")
    }
  }

  const openInviteModal = () => {
    setShowInviteModal(true)
    loadAvailableContacts()
  }

  const handleUpdateMessage = async () => {
    if (newMessage.trim() === "") {
      alert("Введите сообщение перед изменением.")
      return
    }

    if (!connection || !isConnected) {
      alert("Нет соединения с сервером.")
      return
    }

    try {
      console.log("Updating message via SignalR:", editingMessageId, newMessage)
      await connection.invoke("UpdateMessage", editingMessageId, newMessage)

      // Сбрасываем состояние редактирования сразу после отправки
      setEditingMessageId(null)
      setNewMessage("")
    } catch (error) {
      console.error("Ошибка при изменении текста сообщения через SignalR:", error)
      alert("Ошибка при изменении текста сообщения.")
    }
  }

  const handleSendMessage = async () => {
    if (newMessage.trim() === "") {
      alert("Введите сообщение перед отправкой.")
      return
    }
    try {
      console.log("Sending message via SignalR:", newMessage)

      // Если отвечаем на сообщение, добавляем информацию об ответе
      if (replyingToMessage) {
        const replyText = `[Ответ на: ${replyingToMessage.text.substring(0, 50)}${replyingToMessage.text.length > 50 ? "..." : ""}] ${newMessage}`
        console.log("Sending reply message:", replyText)
        await connection.invoke("SendMessage", chat.chatId || chat.id, replyText)
        setReplyingToMessage(null) // Сбрасываем ответ
      } else {
        console.log("Sending regular message:", newMessage)
        await connection.invoke("SendMessage", chat.chatId || chat.id, newMessage)
      }

      setNewMessage("")
    } catch (error) {
      console.error("Ошибка при отправке сообщения через SignalR:", error)
      alert("Ошибка при отправке сообщения.")
    }
  }

  const startEditingMessage = (message) => {
    setEditingMessageId(message.id)
    setNewMessage(message.text)
  }

  const startReplyToMessage = (message) => {
    setReplyingToMessage(message)
    // Фокусируемся на поле ввода
    document.querySelector(".chat-input-area textarea")?.focus()
  }

  const handleDeleteMessage = async (message) => {
    // Подтверждение удаления
    const confirmDelete = window.confirm(
      `Вы уверены, что хотите удалить это сообщение?\n\n"${message.text.substring(0, 100)}${message.text.length > 100 ? "..." : ""}"`,
    )

    if (!confirmDelete) return

    try {
      await deleteMessage(message.id)

      // Удаляем сообщение из локального состояния
      setMessages((prevMessages) => prevMessages.filter((msg) => msg.id !== message.id))

      // Если удаляемое сообщение редактировалось, сбрасываем режим редактирования
      if (editingMessageId === message.id) {
        setEditingMessageId(null)
        setNewMessage("")
      }

      // Если удаляемое сообщение было выбрано для ответа, сбрасываем ответ
      if (replyingToMessage && replyingToMessage.id === message.id) {
        setReplyingToMessage(null)
      }

      console.log("Message deleted successfully:", message.id)
    } catch (error) {
      console.error("Ошибка при удалении сообщения:", error)
      alert("Ошибка при удалении сообщения. Попробуйте еще раз.")
    }
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
        <h3>{chat?.name || chat?.chatName || chat?.chatName || "Чат"}</h3>
        <button onClick={onClose}>Закрыть</button>
      </div>

      <div className="chat-content">
        <div className="chat-messages-section">
          <div className="chat-messages">
            {loading ? (
              <p>Загрузка сообщений...</p>
            ) : messages.length > 0 ? (
              (() => {
                return messages.map((message, index) => {
                  const displayName = participantNameMap.get(message.senderName) || message.senderName || "Unknown"
                  const isEditing = editingMessageId === message.id
                  const isOwnMessage = message.senderId === currentUserId

                  console.log(`Message ${index}:`, {
                    id: message.id,
                    senderId: message.senderId,
                    currentUserId: currentUserId,
                    isOwnMessage: isOwnMessage,
                    status: message.status,
                  })

                  return (
                    <div key={index} className={`chat-message${isEditing ? " editing-message" : ""}`}>
                      <div className="message-content">
                        <div className="message-text-container">
                          <span className="message-text">
                            <strong>{displayName}:</strong> {message.text}
                          </span>
                          <div className="message-meta">
                            <span className="message-time">{formatMessageTime(message.timestamp)}</span>
                            {isOwnMessage && (
                              <span className="message-status">
                                {(message.status === "Sent" || message.status === undefined) && (
                                  <i className="bi bi-check message-status-sent" title="Отправлено"></i>
                                )}
                                {message.status === "Delivered" && (
                                  <>
                                    <i className="bi bi-check message-status-delivered" title="Доставлено"></i>
                                    <i className="bi bi-check message-status-delivered-second"></i>
                                  </>
                                )}
                                {message.status === "Read" && (
                                  <>
                                    <i className="bi bi-check message-status-read" title="Прочитано"></i>
                                    <i className="bi bi-check message-status-read-second"></i>
                                  </>
                                )}
                              </span>
                            )}
                          </div>
                        </div>
                        <div className="message-actions">
                          <button
                            className="reply-message-button btn btn-link btn-sm"
                            title="Ответить на сообщение"
                            onClick={() => startReplyToMessage(message)}
                          >
                            <i className="bi bi-reply"></i>
                          </button>
                          <button
                            className="edit-message-button btn btn-link btn-sm"
                            title="Редактировать сообщение"
                            onClick={() => startEditingMessage(message)}
                          >
                            <i className="bi bi-pencil"></i>
                          </button>
                          <button
                            className="delete-message-button btn btn-link btn-sm"
                            title="Удалить сообщение"
                            onClick={() => handleDeleteMessage(message)}
                          >
                            <i className="bi bi-trash"></i>
                          </button>
                        </div>
                      </div>
                    </div>
                  )
                })
              })()
            ) : (
              <p>Сообщений пока нет.</p>
            )}
          </div>

          <div className="chat-input-area">
            {replyingToMessage && (
              <div className="reply-preview">
                <div className="reply-content">
                  <i className="bi bi-reply reply-icon"></i>
                  <div className="reply-info">
                    <div className="reply-to">
                      В ответ{" "}
                      {participantNameMap.get(replyingToMessage.senderName) ||
                        replyingToMessage.senderName ||
                        "Unknown"}
                    </div>
                    <div className="reply-text">
                      {replyingToMessage.text.length > 100
                        ? replyingToMessage.text.substring(0, 100) + "..."
                        : replyingToMessage.text}
                    </div>
                  </div>
                </div>
                <button className="reply-cancel" onClick={() => setReplyingToMessage(null)} title="Отменить ответ">
                  <i className="bi bi-x"></i>
                </button>
              </div>
            )}
            <textarea
              value={newMessage}
              onChange={(e) => setNewMessage(e.target.value)}
              placeholder="Введите сообщение"
              rows={3}
            />
            <button ref={emojiButtonRef} className="emoji-trigger" onClick={() => setShowEmojiPicker(!showEmojiPicker)}>
              😊
            </button>
            <button onClick={editingMessageId ? handleUpdateMessage : handleSendMessage}>
              {editingMessageId ? "Изменить" : "Отправить"}
            </button>
            {showEmojiPicker && (
              <div className="emoji-picker-wrapper">
                <EmojiPicker
                  onEmojiClick={(emojiData) => {
                    setNewMessage((prev) => prev + emojiData.emoji)
                    setShowEmojiPicker(false)
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

        <div className="chat-participants-column">
          <h4>Участники</h4>
          <ul>
            {participants.length > 0 ? (
              Array.from(new Map(participants.map((p) => [p.id, p])).values()).map((participant) => {
                return (
                  <li
                    key={participant.id}
                    style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}
                  >
                    <span>{participant.firstName || participant.phone || "Участник"}</span>
                    <div style={{ display: "flex", gap: "5px", alignItems: "center" }}>
                      {/* Кнопка профиля участника */}
                      <button
                        onClick={() => setProfileUserId(participant.id)}
                        className="btn btn-link btn-sm"
                        style={{ padding: "2px 6px", color: "#007bff" }}
                        title="Открыть профиль"
                      >
                        <i className="bi bi-person-circle"></i>
                      </button>

                      {/* Кнопка удаления из чата (только для создателя чата) */}
                      {currentUserId === chat.creatorUserId && participant.id !== chat.creatorUserId && (
                        <button
                          onClick={() => handleRemoveUser(participant)}
                          className="btn btn-link btn-sm"
                          style={{ padding: "2px 6px", color: "#dc3545" }}
                          title="Удалить из чата"
                        >
                          <i className="bi bi-x-lg"></i>
                        </button>
                      )}
                    </div>
                  </li>
                )
              })
            ) : (
              <li>Нет участников</li>
            )}
          </ul>
          <button className="invite-button" onClick={openInviteModal}>
            Пригласить в чат
          </button>
        </div>
      </div>
      {showInviteModal && (
        <div className="modal-overlay" onClick={() => setShowInviteModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h3>Пригласить в чат</h3>
            {loadingContacts ? (
              <p>Загрузка контактов...</p>
            ) : availableContacts.length > 0 ? (
              <div className="contacts-list">
                {availableContacts.map((contact) => (
                  <div key={contact.id} className="contact-item">
                    <span>{contact.firstName || contact.phone || "Контакт"}</span>
                    <button onClick={() => handleInviteToChat(contact.id)} className="invite-contact-button">
                      Пригласить
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <p>Нет доступных контактов для приглашения</p>
            )}
            <div className="buttons-container">
              <button onClick={() => setShowInviteModal(false)} className="cancel-button">
                Закрыть
              </button>
            </div>
          </div>
        </div>
      )}
      {profileUserId && (
        <ProfileModal
          userId={profileUserId}
          onClose={() => setProfileUserId(null)}
          onContactDeleted={() => {
            // Обновляем список участников после удаления контакта
            const fetchParticipants = async () => {
              try {
                const users = await getChatUsersList(chat.chatId || chat.id)
                const uniqueUsers = Array.from(new Map(users.map((u) => [u.id, u])).values())
                setParticipants(uniqueUsers)
              } catch (error) {
                console.error("Ошибка при загрузке участников чата:", error)
              }
            }
            fetchParticipants()
            setProfileUserId(null)
          }}
        />
      )}
    </div>
  )
}

export default ChatWindow