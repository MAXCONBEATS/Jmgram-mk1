import React from 'react';
import UserSearch from './UserSearch';
import '../css/ContactPanel.css';

function ContactsPanel({ userId, contacts, contactRequests, onAcceptContactRequest, refreshTrigger, setProfileUserId, refreshContactsAndChats }) {

  return (
    <div className="contacts-panel">
      <div className="contacts-header-container">
        <h2 className="contacts-header">Контакты</h2>
      </div>

      <div className="user-search-container">
        <UserSearch />
      </div>

      <div className="contacts-wrapper">
        <div className="contacts-container">
          <ul className="contact-list">
            {contacts.length > 0 ? (
              contacts.map((contact) => (
                <li
                  key={contact.contactUserId}
                  onClick={() => setProfileUserId(contact.contactUserId)}
                  title={`Открыть профиль ${contact.name}`}
                >
                  {contact.name}
                </li>
              ))
            ) : (
              <li className="no-contacts">У вас пока нет контактов</li>
            )}
          </ul>
        </div>

        {contactRequests.length > 0 && (
          <div className="contact-requests-container">
            <h3>Запросы в контакты:</h3>
            <ul className="contact-requests-list">
              {contactRequests.map((request) => (
                <li key={request.id}>
                  <span>{request.senderName || 'Неизвестный пользователь'}</span>
                  <button onClick={async () => {
                    await onAcceptContactRequest(request.id);
                    refreshContactsAndChats();
                  }}>Принять</button>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}

export default ContactsPanel;
