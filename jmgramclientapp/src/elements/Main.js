import React, { useState, useEffect } from 'react';
import '../css/Main.css';
import '../css/ContextMenu.css';
import axios from 'axios';
import ChatListContainer from './ChatListContainer';
import CreateChatButton from './CreateChatButton';
import ChatWindow from './ChatWindow';
import ContactsPanel from './ContactsPanel';
import NotificationsPanel from './NotificationsPanel';
import ProfileModal from './ProfileModal';
import { getContactList, getContactRequests, acceptContactRequest } from '../controllers/ContactController';

axios.defaults.baseURL = 'https://localhost:5087';

function Main({ error, onLogout }) {
  const [selectedChat, setSelectedChat] = useState(null);
  const [refreshChats, setRefreshChats] = useState(false);
  const [profileUserId, setProfileUserId] = useState(null);
  const [refreshContacts, setRefreshContacts] = useState(false);

  const [contacts, setContacts] = useState([]);
  const [contactRequests, setContactRequests] = useState([]);

  const userId = localStorage.getItem('UserId');
  const refreshContactsAndChats = async () => {
    try {
      const updatedContacts = await getContactList();
      setContacts(updatedContacts);
      setRefreshChats(prev => !prev);
    } catch (error) {
      console.error('Ошибка при обновлении контактов и чатов:', error);
    }
  };

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
  }, [refreshContacts]);

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
  }, [refreshContacts, userId]);

  const handleAcceptContactRequest = async (contactRequestId) => {
    try {
      const result = await acceptContactRequest(contactRequestId);
      if (typeof result === 'string' || (result && result.isSuccess)) {
        setContactRequests((prev) => prev.filter((req) => req.id !== contactRequestId));
        await refreshContactsAndChats();
      } else {
        alert(`Ошибка при принятии запроса: ${result.errorMessage || 'Неизвестная ошибка'}`);
      }
    } catch (error) {
      console.error('Ошибка при принятии запроса в контакты:', error);
    }
  };

  const handleCreateChat = (newChat) => {
    console.log('handleCreateChat called with newChat:', newChat);
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
        <ContactsPanel 
          userId={userId} 
          contacts={contacts} 
          contactRequests={contactRequests} 
          onAcceptContactRequest={handleAcceptContactRequest} 
          refreshTrigger={refreshContacts} 
          setProfileUserId={setProfileUserId} 
          refreshContactsAndChats={refreshContactsAndChats}
        />

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
      <NotificationsPanel />

      {selectedChat && (
        <ChatWindow
          chat={selectedChat}
          onClose={() => setSelectedChat(null)}
          senderId={userId}
          senderName={localStorage.getItem('UserName')}
          currentUserId={userId}
        />
      )}

      {profileUserId && (
        <ProfileModal
          userId={profileUserId}
          onClose={() => setProfileUserId(null)}
          onContactDeleted={refreshContactsAndChats}
        />
      )}
    </div>
  );
}

export default Main;
