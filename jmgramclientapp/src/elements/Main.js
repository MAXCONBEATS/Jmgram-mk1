import React, { useState, useEffect } from 'react';
import '../css/Main.css';
import '../css/ContextMenu.css';
import logoutIcon from '../assets/images/logout_icon.png';
import { getContactList, getContactRequests, acceptContactRequest } from '../controllers/ContactController';
import { getNotifications } from '../controllers/NotificationController';
import UserSearch from './UserSearch';
import ChatWindow from './ChatWindow';
import NotificationWindow from './NotificationWindow';
import ChatListContainer from './ChatListContainer';
import axios from 'axios';
import CreateChatButton from './CreateChatButton';
axios.defaults.baseURL = 'https://localhost:5087';

function Main({ error, onLogout }) {
  const [contacts, setContacts] = useState([]);
  const [contactRequests, setContactRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedChat, setSelectedChat] = useState(null);
  const [refreshChats, setRefreshChats] = useState(false);

  const [contextMenu, setContextMenu] = useState({ visible: false, x: 0, y: 0, chatId: null });

  const userId = localStorage.getItem('UserId');
  console.log('Main.js userId from localStorage:', userId);

  useEffect(() => {
    const fetchContacts = async () => {
      try {
        const contactList = await getContactList();
        setContacts(contactList);
      } catch (error) {
        console.error('Ошибка при получении списка контактов:', error);
      }
    };
    fetchContacts();
  }, []);

  useEffect(() => {
    if (contacts.length === 0) return;

    const fetchContactRequests = async () => {
      try {
        const requestsData = await getContactRequests();
        const mappedRequests = requestsData.map((request) => {
          let name = 'Неизвестный пользователь';
          if (request.senderUserId === userId) {
            const recipientContact = contacts.find(contact => contact.contactUserId === request.recipientUserId);
            if (recipientContact && recipientContact.name) {
              name = recipientContact.name;
            } else {
              name = 'Неизвестный номер';
            }
          } else {
            const senderContact = contacts.find(contact => contact.contactUserId === request.senderUserId);
            if (senderContact && senderContact.name) {
              name = senderContact.name;
            } else {
              name = 'Неизвестный номер';
            }
          }
          return {
            ...request,
            senderName: name,
          };
        });
        setContactRequests(mappedRequests);
      } catch (error) {
        console.error('Ошибка при получении запросов в контакты:', error);
      }
    };
    fetchContactRequests();
  }, [contacts]);

  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    const fetchNotifications = async () => {
      try {
        const data = await getNotifications();
        setNotifications(data);
      } catch (error) {
        console.error('Ошибка при получении уведомлений:', error);
      }
    };
    fetchNotifications();
    const interval = setInterval(fetchNotifications, 30000);
    return () => clearInterval(interval);
  }, []);

  const handleAcceptContactRequest = async (contactRequestId) => {
    try {
      const result = await acceptContactRequest(contactRequestId);
      if (typeof result === 'string' || (result && result.isSuccess)) {
        setContactRequests((prev) => prev.filter((req) => req.id !== contactRequestId));
        const updatedContacts = await getContactList();
        setContacts(updatedContacts);
      } else {
        alert(`Ошибка при принятии запроса: ${result.errorMessage || 'Неизвестная ошибка'}`);
      }
    } catch (error) {
      console.error('Ошибка при принятии запроса в контакты:', error);
    }
  };

  const handleRemoveNotification = async (id) => {
    try {
      await axios.delete('/Notification/Delete', { params: { id }, withCredentials: true });
      setNotifications((prev) => prev.filter((notif) => notif.Id !== id));
    } catch (error) {
      console.error('Ошибка при удалении уведомления:', error.response || error);
      alert('Не удалось удалить уведомление.');
    }
  };

  const handleCreateChat = (newChat) => {
    setRefreshChats(prev => !prev);
    setSelectedChat(newChat);
  };

  return (
    <div className="main-container">
      <img src={logoutIcon} alt="Выйти" className="logout-icon" onClick={onLogout} />

      {error && <p className="error-message">{error}</p>}

      <div className="main-content" style={{ display: 'flex', gap: '20px' }}>
        <div style={{ flex: '1', maxWidth: '200px' }}>
          <h2 className="contacts-header">Контакты:</h2>
          <div className="user-search-container">
            <UserSearch />
          </div>

          <div className="contacts-wrapper">
            <div className="contacts-container">
              <ul className="contact-list">
                {contacts.length > 0 ? (
                  contacts.map((contact) => (
                    <li key={contact.contactUserId}>{contact.name}</li>
                  ))
                ) : (
                  <li>У вас пока нет контактов</li>
                )}
              </ul>
            </div>
            {contactRequests.length > 0 && (
              <div className="contact-requests-container" style={{ marginTop: '10px' }}>
                <h3>Запросы в контакты:</h3>
                <ul className="contact-requests-list">
                  {contactRequests.map((request) => (
                    <li key={request.id}>
                      {request.senderName || 'Неизвестный пользователь'}
                      <button onClick={() => handleAcceptContactRequest(request.id)} style={{ marginLeft: '10px' }}>
                        Принять
                      </button>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        </div>

        <div style={{ flex: '2' }}>
          <h2>Чаты:</h2>
          <ChatListContainer key={refreshChats} selectedChat={selectedChat} setSelectedChat={setSelectedChat} />
          <CreateChatButton contacts={contacts} onCreateChat={handleCreateChat} />
        </div>

        {selectedChat && (
          <div className="chat-overlay" onClick={() => setSelectedChat(null)}>
            <div onClick={e => e.stopPropagation()}>
              <ChatWindow chat={selectedChat} onClose={() => setSelectedChat(null)} senderId={userId} senderName={localStorage.getItem('UserName')} currentUserId={userId} />
            </div>
          </div>
        )}
      </div>
      <NotificationWindow notifications={notifications} onRemoveNotification={handleRemoveNotification} />
    </div>
  );
}

export default Main;
