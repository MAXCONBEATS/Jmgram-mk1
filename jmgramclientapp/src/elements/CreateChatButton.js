"use client"

import { useState } from "react"
import { createChat } from "../controllers/ChatController"
import "../css/CreateChatButton.css"

function CreateChatButton({ contacts, onCreateChat }) {
  const [isOpen, setIsOpen] = useState(false)
  const [chatName, setChatName] = useState("")
  const [selectedContacts, setSelectedContacts] = useState([])
  const [chatType, setChatType] = useState("Group") // Строка для UI

  const toggleModal = () => {
    setIsOpen(!isOpen)
    if (!isOpen) {
      setChatName("")
      setSelectedContacts([])
      setChatType("Group")
    }
  }

  const handleCheckboxChange = (contactId) => {
    if (selectedContacts.includes(contactId)) {
      setSelectedContacts(selectedContacts.filter((id) => id !== contactId))
    } else {
      setSelectedContacts([...selectedContacts, contactId])
    }
  }

  const handleCreateChat = async () => {
    if (!chatName.trim()) {
      alert("Пожалуйста, введите название чата.")
      return
    }
    if (selectedContacts.length === 0) {
      alert("Пожалуйста, выберите хотя бы одного участника.")
      return
    }

    try {
      const phones = contacts
        .filter((contact) => selectedContacts.includes(contact.contactUserId))
        .map((contact) => contact.phone)
        .filter((phone) => phone)

      // ИСПРАВЛЕННЫЙ payload: ChatType внутри объекта chat как число
      const payload = {
        chat: {
          name: chatName,
          chatType: chatType === "Channel" ? 1 : 0, // ← ПРЕОБРАЗУЕМ в число: 0 = Group, 1 = Channel
        },
        phones: phones,
      }

      console.log("Создаем чат с параметрами:", payload)

      const newChat = await createChat(payload)
      onCreateChat(newChat)
      toggleModal()
    } catch (error) {
      console.error("Ошибка при создании чата:", error)
      alert("Не удалось создать чат. Попробуйте еще раз.")
    }
  }

  return (
    <>
      <button className="create-chat-button" onClick={toggleModal}>
        Создать чат
      </button>

      {isOpen && (
        <div className="modal-overlay" onClick={toggleModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h3>Создать новый чат</h3>

            <input
              type="text"
              placeholder="Название чата"
              value={chatName}
              onChange={(e) => setChatName(e.target.value)}
              className="modal-input"
            />

            <div className="chat-type-section">
              <p className="section-title">Тип чата:</p>
              <div className="chat-type-options">
                <label className="radio-label">
                  <input
                    type="radio"
                    name="chatType"
                    value="Group"
                    checked={chatType === "Group"}
                    onChange={(e) => setChatType(e.target.value)}
                  />
                  <span className="radio-text">
                    <strong>👥 Группа</strong>
                    <small>Все участники могут писать сообщения</small>
                  </span>
                </label>

                <label className="radio-label">
                  <input
                    type="radio"
                    name="chatType"
                    value="Channel"
                    checked={chatType === "Channel"}
                    onChange={(e) => setChatType(e.target.value)}
                  />
                  <span className="radio-text">
                    <strong>📢 Канал</strong>
                    <small>Только вы можете писать, остальные читают</small>
                  </span>
                </label>
              </div>
            </div>

            <div className="contacts-list">
              <p className="section-title">Выберите {chatType === "Channel" ? "подписчиков" : "участников"}:</p>
              {contacts.length > 0 ? (
                <div className="contacts-grid">
                  {contacts.map((contact) => (
                    <label key={contact.contactUserId} className="checkbox-label">
                      <input
                        type="checkbox"
                        checked={selectedContacts.includes(contact.contactUserId)}
                        onChange={() => handleCheckboxChange(contact.contactUserId)}
                      />
                      <span className="contact-name">{contact.name}</span>
                    </label>
                  ))}
                </div>
              ) : (
                <p className="no-contacts">У вас пока нет контактов</p>
              )}
            </div>

            <div className="chat-info">
              {chatType === "Channel" ? (
                <div className="info-box channel-info">
                  <strong>📢 Канал:</strong> Только вы сможете отправлять сообщения. Участники смогут только читать ваши
                  сообщения.
                </div>
              ) : (
                <div className="info-box group-info">
                  <strong>👥 Группа:</strong> Все участники смогут отправлять сообщения и общаться друг с другом.
                </div>
              )}
            </div>

            <div className="buttons-container">
              <button onClick={handleCreateChat} className="create-button">
                Создать {chatType === "Channel" ? "канал" : "группу"}
              </button>
              <button onClick={toggleModal} className="cancel-button">
                Отмена
              </button>
            </div>

          </div>
        </div>
      )}
    </>
  )
}

export default CreateChatButton
