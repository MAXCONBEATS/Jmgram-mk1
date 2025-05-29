import React, { useState } from 'react';
import '../css/Main.css';
import '../css/ContextMenu.css';
import axios from 'axios';
import ChatListContainer from './ChatListContainer';
import CreateChatButton from './CreateChatButton';
import ChatWindow from './ChatWindow';
import ContactsPanel from './ContactsPanel';
import NotificationsPanel from './NotificationsPanel';
import ProfileModal from './ProfileModal';

axios.defaults.baseURL = 'https://localhost:5087';

function Main({ error, onLogout }) {
  const [selectedChat, setSelectedChat] = useState(null);
  const [refreshChats, setRefreshChats] = useState(false);
  const [profileUserId, setProfileUserId] = useState(null);
  const [refreshContacts, setRefreshContacts] = useState(false);

  const userId = localStorage.getItem('UserId');

  const refreshContactsAndChats = () => {
    setRefreshContacts(prev => !prev);
    setRefreshChats(prev => !prev);
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
        <ContactsPanel userId={userId} refreshTrigger={refreshContacts} setProfileUserId={setProfileUserId} />

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
            <CreateChatButton contacts={[]} onCreateChat={handleCreateChat} />
          </div>
        </div>
      </div>

      <NotificationsPanel />

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
