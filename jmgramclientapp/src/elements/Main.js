import React, { useState, useEffect } from 'react';
import '../css/Main.css';
import '../css/ContextMenu.css';
import { getContactList, getContactRequests, acceptContactRequest } from '../controllers/ContactController';
import { getNotifications } from '../controllers/NotificationController';
import UserSearch from './UserSearch';
import ChatWindow from './ChatWindow';
import NotificationWindow from './NotificationWindow';
import ChatListContainer from './ChatListContainer';
import axios from 'axios';
import { markAsRead } from '../controllers/NotificationController';
import CreateChatButton from './CreateChatButton';
import UserProfile from './UserProfile';
axios.defaults.baseURL = 'https://localhost:5087';

function Main({ error, onLogout }) {
  const [contacts, setContacts] = useState([]);
  const [contactRequests, setContactRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedChat, setSelectedChat] = useState(null);
  const [refreshChats, setRefreshChats] = useState(false);
  const [profileUserId, setProfileUserId] = useState(null); 

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
    const fetchContactRequests = async () => {
      try {
        const requestsData = await getContactRequests();
        const mappedRequests = requestsData.map((request) => {
          let name = 'Неизвестный пользователь';
          if (request.senderUserId === userId) {
            name = 'Неизвестный номер';
          } else {
            name = 'Неизвестный номер';
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
  }, []);

  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    const fetchNotifications = async () => {
      try {
        const data = await getNotifications();
        const unreadNotifications = data.filter(notification => !notification.isRead);
        setNotifications(unreadNotifications);
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

  const handleCloseNotification = (id) => {
    setNotifications((prev) => prev.filter((notif) => notif.Id !== id));
  };

  const handleMarkAsReadNotification = async (id) => {
    try {
      const success = await markAsRead(id);
      if (success) {
        setNotifications((prev) => prev.filter((notif) => notif.Id !== id));
      }
    } catch (error) {
      console.error('Ошибка при пометке уведомления как прочитанного:', error);
    }
  };

  const handleCreateChat = (newChat) => {
    setRefreshChats(prev => !prev);
    setSelectedChat(newChat);
  };

return (
  <div className="main-container" style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>

    <button onClick={() => setProfileUserId(userId)} className="btn btn-outline-light btn-sm"
  style={{ fontSize: '0.8rem', padding: '4px 8px', cursor: 'pointer' }}>Мой профиль</button>

    <div style={{ display: 'flex', justifyContent: 'flex-end', padding: '10px' }}>
  <i className="bi bi-box-arrow-right logout-icon" onClick={onLogout} 
    style={{ cursor: 'pointer', fontSize: '24px', color: 'white' }} title="Выйти"></i>
</div>

    {error && <p className="error-message" style={{ textAlign: 'center', color: '#ff5555', margin: '10px 0' }}>{error}</p>}

    <div className="main-content" style={{ 
      display: 'flex', 
      flex: 1,
      gap: '30px',
      padding: '20px',
      overflow: 'hidden'
    }}>
      {/* Левая колонка (Контакты) */}
      <div style={{ 
        flex: '0 0 250px',
        display: 'flex',
        flexDirection: 'column',
        borderRight: '1px solid #424242',
        paddingRight: '20px',
        height: '100%'
      }}>
        <div style={{ 
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: '15px'
        }}>
          <h2 className="contacts-header">Контакты</h2>
        </div>

        <div className="user-search-container" style={{ marginBottom: '20px' }}>
          <UserSearch />
        </div>

        <div className="contacts-wrapper" style={{ 
          flex: 1,
          overflowY: 'auto',
          paddingRight: '5px'
        }}>
          <div className="contacts-container">
            <ul className="contact-list" style={{ listStyle: 'none', padding: 0,margin: 0}}>
              {contacts.length > 0 ? (
                contacts.map((contact) => (
                  <li
                    key={contact.contactUserId}
                    onClick={() => setProfileUserId(contact.contactUserId)}
                    style={{ 
                      cursor: 'pointer',
                      padding: '8px 12px',
                      marginBottom: '5px',
                      borderRadius: '4px',
                      backgroundColor: '#212121',
                      color: 'white',
                      transition: 'background-color 0.2s'
                    }}
                    onMouseOver={(e) => e.target.style.backgroundColor = '#424242'}
                    onMouseOut={(e) => e.target.style.backgroundColor = '#212121'}
                    title={`Открыть профиль ${contact.name}`}
                  >
                    {contact.name}
                  </li>
                ))
              ) : (
                <li style={{ 
                  padding: '8px 12px',
                  color: '#bdbdbd'
                }}>У вас пока нет контактов</li>
              )}
            </ul>
          </div>

          {contactRequests.length > 0 && (
            <div className="contact-requests-container" style={{ 
              marginTop: '20px',
              backgroundColor: '#212121',
              padding: '12px',
              borderRadius: '4px'
            }}>
              <h3 style={{ 
                marginTop: 0,
                marginBottom: '10px',
                color: 'white'
              }}>Запросы в контакты:</h3>
              <ul className="contact-requests-list" style={{ 
                listStyle: 'none',
                padding: 0,
                margin: 0
              }}>
                {contactRequests.map((request) => (
                  <li key={request.id} style={{ 
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    padding: '8px 0',
                    borderBottom: '1px solid #424242'
                  }}>
                    <span style={{ color: 'white' }}>{request.senderName || 'Неизвестный пользователь'}</span>
                    <button 
                      onClick={() => handleAcceptContactRequest(request.id)} 
                      style={{ 
                        marginLeft: '10px',
                        background: '#424242',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        padding: '4px 8px',
                        cursor: 'pointer'
                      }}>Принять</button>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>

      {/* Правая колонка (Чаты) */}
      <div style={{ 
        flex: 1,
        display: 'flex',
        flexDirection: 'column',
        minWidth: 0 
      }}>
        <h2 style={{ 
          marginTop: 0,
          marginBottom: '15px',
          color: 'white'
        }}>Чаты</h2>
        
        <div style={{ 
          flex: 1,
          overflowY: 'auto',
          marginBottom: '15px'
        }}>
          <ChatListContainer 
            key={refreshChats} 
            selectedChat={selectedChat} 
            setSelectedChat={setSelectedChat} 
          />
        </div>
        
        <div style={{ 
          marginTop: 'auto',
          paddingTop: '15px'
        }}>
          <CreateChatButton contacts={contacts} onCreateChat={handleCreateChat} />
        </div>
      </div>
    </div>

    {/* Всплывающие окна */}
    {selectedChat && (
      <div className="chat-overlay" style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(24, 24, 24, 0.8)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000
      }} onClick={() => setSelectedChat(null)}>
        <div onClick={e => e.stopPropagation()}>
          <ChatWindow chat={selectedChat} onClose={() => setSelectedChat(null)} 
            senderId={userId} senderName={localStorage.getItem('UserName')} 
            currentUserId={userId} />
        </div>
      </div>
    )}

    <NotificationWindow 
      notifications={notifications} 
      onCloseNotification={handleCloseNotification} 
      onMarkAsReadNotification={handleMarkAsReadNotification} 
    />

    {profileUserId && (
      <UserProfile userId={profileUserId} onClose={() => setProfileUserId(null)} />
    )}
  </div>
);
}

export default Main;
