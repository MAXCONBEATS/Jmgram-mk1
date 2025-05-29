import React, { useState } from 'react';
import { createChat } from '../controllers/ChatController';
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
        <div className="modal-overlay" onClick={toggleModal} style={overlayStyle}>
          <div className="modal-content" onClick={e => e.stopPropagation()} style={modalStyle}>
            <h3>Создать новый чат</h3>
            <input
              type="text"
              placeholder="Название чата"
              value={chatName}
              onChange={(e) => setChatName(e.target.value)}
              style={inputStyle}
            />
            <div style={contactsListStyle}>
              <p>Выберите участников:</p>
              {contacts.length > 0 ? (
                contacts.map(contact => (
                  <label key={contact.contactUserId} style={checkboxLabelStyle}>
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
            <div style={buttonsContainerStyle}>
              <button onClick={handleCreateChat} style={createButtonStyle}>Создать</button>
              <button onClick={toggleModal} style={cancelButtonStyle}>Отмена</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

const overlayStyle = {
  position: 'fixed',
  top: 0, left: 0, right: 0, bottom: 0,
  backgroundColor: 'rgba(0,0,0,0.5)',
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  zIndex: 1000,
};

const modalStyle = {
  backgroundColor: '#212121',
  padding: '20px',
  borderRadius: '10px',
  width: '400px',
  maxHeight: '80vh',
  overflowY: 'auto',
  color: 'white',
  display: 'flex',
  flexDirection: 'column',
  gap: '10px',
};

const inputStyle = {
  padding: '10px',
  borderRadius: '5px',
  border: '1px solid #555',
  backgroundColor: '#333',
  color: 'white',
  fontSize: '1em',
};

const contactsListStyle = {
  maxHeight: '200px',
  overflowY: 'auto',
  border: '1px solid #555',
  borderRadius: '5px',
  padding: '10px',
};

const checkboxLabelStyle = {
  display: 'block',
  marginBottom: '5px',
  cursor: 'pointer',
};

const buttonsContainerStyle = {
  display: 'flex',
  justifyContent: 'flex-end',
  gap: '10px',
};

const createButtonStyle = {
  backgroundColor: '#007bff',
  color: 'white',
  border: 'none',
  borderRadius: '5px',
  padding: '10px 20px',
  cursor: 'pointer',
  fontSize: '1em',
};

const cancelButtonStyle = {
  backgroundColor: '#555',
  color: 'white',
  border: 'none',
  borderRadius: '5px',
  padding: '10px 20px',
  cursor: 'pointer',
  fontSize: '1em',
};

export default CreateChatButton;
