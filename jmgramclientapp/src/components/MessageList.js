import { formatMessageTime, renderMediaContent } from '../utils/messageUtils'

function MessageList({ 
  messages, 
  loading, 
  participantNameMap, 
  currentUserId, 
  editingMessageId,
  onStartEditing,
  onStartReply,
  onDeleteMessage
}) {
  if (loading) {
    return <p>Загрузка сообщений...</p>
  }

  if (messages.length === 0) {
    return <p>Сообщений пока нет.</p>
  }

  return (
    <>
      {messages.map((message, index) => {
        const displayName = participantNameMap.get(message.senderName) || message.senderName || "Unknown"
        const isEditing = editingMessageId === message.id
        const isOwnMessage = message.senderId === currentUserId

        console.log(`Рендерим сообщение ${index}:`, message);

        return (
          <div key={index} className={`chat-message${isEditing ? " editing-message" : ""}`}>
            <div className="message-content">
              <div className="message-text-container">
                <div className="message-text">
                  <strong>{displayName}: </strong>
                  {renderMediaContent(message)}
                </div>
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
                  onClick={() => onStartReply(message)}
                >
                  <i className="bi bi-reply"></i>
                </button>
                <button
                  className="edit-message-button btn btn-link btn-sm"
                  title="Редактировать сообщение"
                  onClick={() => onStartEditing(message)}
                >
                  <i className="bi bi-pencil"></i>
                </button>
                <button
                  className="delete-message-button btn btn-link btn-sm"
                  title="Удалить сообщение"
                  onClick={() => onDeleteMessage(message)}
                >
                  <i className="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        )
      })}
    </>
  )
}

export default MessageList