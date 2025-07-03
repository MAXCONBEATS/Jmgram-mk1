import { useState } from 'react'
import { removeUserFromChat, InviteToChat, getChatUsersList } from '../controllers/ChatController'
import { getContactList } from '../controllers/ContactController'

function ParticipantsList({ 
  participants, 
  chat, 
  currentUserId, 
  onParticipantsUpdate,
  onOpenProfile 
}) {
  const [showInviteModal, setShowInviteModal] = useState(false)
  const [availableContacts, setAvailableContacts] = useState([])
  const [loadingContacts, setLoadingContacts] = useState(false)

  const handleRemoveUser = async (participantToRemove) => {
    if (!participantToRemove) return
    try {
      await removeUserFromChat(chat.chatId || chat.id, participantToRemove.id)
      onParticipantsUpdate?.()
    } catch (error) {
      console.error("Error removing user:", error)
    }
  }

  const loadAvailableContacts = async () => {
    setLoadingContacts(true)
    try {
      const allContacts = await getContactList()
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
      onParticipantsUpdate?.()
    } catch (error) {
      console.error("Ошибка при отправке приглашения:", error)
      alert("Ошибка при отправке приглашения")
    }
  }

  const openInviteModal = () => {
    setShowInviteModal(true)
    loadAvailableContacts()
  }

  return (
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
                  <button
                    onClick={() => onOpenProfile(participant.id)}
                    className="btn btn-link btn-sm"
                    style={{ padding: "2px 6px", color: "#007bff" }}
                    title="Открыть профиль"
                  >
                    <i className="bi bi-person-circle"></i>
                  </button>
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
    </div>
  )
}

export default ParticipantsList