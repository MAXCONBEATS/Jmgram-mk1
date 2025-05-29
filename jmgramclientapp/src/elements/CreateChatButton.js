import React, { useState } from 'react';
import { createChat } from '../controllers/ChatController';
import '../css/CreateChatButton.css';
function CreateChatButton({ contacts, onCreateChat }) {
  const [isOpen, setIsOpen] = useState(false);
  const [chatName, setChatName] = useState('');
  const [selectedContacts, setSelectedContacts] = useState([]);

  const toggleModal = () => {
    setIsOpen(!isOpen);
    if (!isOpen) {
      setChatName('');
      setSelectedContacts([]);
    }
  };

  const handleCheckboxChange = (contactId) => {
    if (selectedContacts.includes(contactId)) {
      setSelectedContacts(selectedContacts.filter(id => id !== contactId));
    } else {
      setSelectedContacts([...selectedContacts, contactId]);
    }
  };

  const handleCreateChat = async () => {
    if (!chatName.trim()) {
      alert('Пожалуйста, введите название чата.');
      return;
    }
    if (selectedContacts.length === 0) {
      alert('Пожалуйста, выберите хотя бы одного участника.');
      return;
    }
    try {
      const phones = contacts
        .filter(contact => selectedContacts.includes(contact.contactUserId))
        .map(contact => contact.phone)
        .filter(phone => phone);

      const payload = {
        chat: { name: chatName },
        phones: phones
      };

      const newChat = await createChat(payload);
      onCreateChat(newChat);
      toggleModal();
    } catch (error) {
      console.error('Ошибка при создании чата:', error);
      alert('Не удалось создать чат. Попробуйте еще раз.');
    }
  };

  return (
    <>
      <button className="create-chat-button" onClick={toggleModal}>Создать чат</button>
      {isOpen && (
        <div className="modal-overlay" onClick={toggleModal}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h3>Создать новый чат</h3>
            <input
              type="text"
              placeholder="Название чата"
              value={chatName}
              onChange={(e) => setChatName(e.target.value)}
              className='modal-input'
            />
            <div className='contacts-list'>
              <p>Выберите участников:</p>
              {contacts.length > 0 ? (
                contacts.map(contact => (
                  <label key={contact.contactUserId} className='checkbox-label'>
                    <input
                      type="checkbox"
                      checked={selectedContacts.includes(contact.contactUserId)}
                      onChange={() => handleCheckboxChange(contact.contactUserId)}
                    />
                    {contact.name}
                  </label>
                ))
              ) : (
                <p>У вас пока нет контактов</p>
              )}
            </div>
            <div className='buttons-container'>
              <button onClick={handleCreateChat} className='create-button'>Создать</button>
              <button onClick={toggleModal} className='cancel-button'>Отмена</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}


export default CreateChatButton;
