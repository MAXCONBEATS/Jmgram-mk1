import { useState, useEffect } from "react"
import { GetChatInvites, ResponseToInvite } from "../controllers/ChatController"
import "../css/ChatInvitationsList.css"

function ChatInvitationsList({ selectedChatInvitation, setSelectedChatInvitation, refreshTrigger }) {
  const [chatInvitations, setChatInvitations] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetchChatInvites = async () => {
      setIsLoading(true)
      setError(null)
      try {
        const invites = await GetChatInvites()
        console.log("GetChatInvites response:", invites)
        console.log("Type of invites:", typeof invites)
        console.log("Is array:", Array.isArray(invites))

        // Проверяем структуру каждого приглашения
        if (Array.isArray(invites)) {
          invites.forEach((invite, index) => {
            console.log(`Invite ${index}:`, invite)
            console.log(`Invite ${index} status:`, invite.status, typeof invite.status)
          })

          // Показываем количество приглашений до и после фильтрации
          const filteredInvites = invites.filter((invite) => invite.status === 0)
          console.log("Total invites:", invites.length)
          console.log("Filtered invites (status === 0):", filteredInvites.length)
          console.log("Filtered invites data:", filteredInvites)
        }

        setChatInvitations(Array.isArray(invites) ? invites : [])
      } catch (error) {
        console.error("Ошибка при получении списка приглашений в чат", error)
        setError("Ошибка при получении списка приглашений в чат.")
      } finally {
        setIsLoading(false)
      }
    }
    fetchChatInvites()
  }, [refreshTrigger])

  const handleResponse = async (chatInvitationId, accepted) => {
    try {
      await ResponseToInvite(chatInvitationId, accepted)
      setChatInvitations((prevInvites) => prevInvites.filter((invite) => invite.id !== chatInvitationId))
      if (selectedChatInvitation && selectedChatInvitation.id === chatInvitationId) {
        setSelectedChatInvitation(null)
      }
    } catch (error) {
      console.error("Ошибка при ответе на приглашение в чат", error)
      alert("Ошибка при ответе на приглашение в чат.")
    }
  }

  const handleChatClick = (chat) => {
    setSelectedChatInvitation(chat)
  }

  if (isLoading) {
    return <div className="chat-invitations-list">Загрузка приглашений...</div>
  }

  if (error) {
    return <div className="chat-invitations-list error">{error}</div>
  }

  // Фильтруем приглашения со статусом 0
  const pendingInvitations = chatInvitations.filter((invite) => {
    console.log("Filtering invite:", invite.id, "status:", invite.status, "type:", typeof invite.status)
    return invite.status === 0
  })

  console.log("Pending invitations after filter:", pendingInvitations)

  if (pendingInvitations.length === 0) {
    return (
      <div className="chat-invitations-list">
        <p>Нет новых приглашений в чаты.</p>
        {chatInvitations.length > 0 && (
          <p style={{ fontSize: "12px", color: "#666" }}>
            Всего приглашений: {chatInvitations.length} (показываются только новые)
          </p>
        )}
      </div>
    )
  }

  return (
    <div className="chat-invitations-list">
      <h3>Приглашения в чаты ({pendingInvitations.length})</h3>
      <ul>
        {pendingInvitations.map((invite) => (
          <li
            key={invite.id}
            className={`chat-invitation-item ${selectedChatInvitation && selectedChatInvitation.id === invite.id ? "selected" : ""}`}
            onClick={() => handleChatClick(invite)}
          >
            <div className="chat-invitation-info">
              <span className="chat-name">{invite.chatName || "Без названия"}</span>
              <span className="sender-name">От: {invite.senderName || "Неизвестный"}</span>
              {/* <span className="status-debug" style={{ fontSize: "10px", color: "#999" }}>
                Status: {invite.status} ({typeof invite.status})
              </span> */}
            </div>
            <div className="chat-invitation-actions">
              <button
                className="accept-btn"
                onClick={(e) => {
                  e.stopPropagation()
                  handleResponse(invite.id, true)
                }}
              >
                Принять
              </button>
              <button
                className="reject-btn"
                onClick={(e) => {
                  e.stopPropagation()
                  handleResponse(invite.id, false)
                }}
              >
                Отклонить
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}

export default ChatInvitationsList
