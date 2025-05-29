import React from 'react';
import UserProfile from './UserProfile';

function ProfileModal({ userId, onClose, onContactDeleted }) {
  return (
    <UserProfile 
      userId={userId} 
      onClose={onClose} 
      onContactDeleted={onContactDeleted} 
    />
  );
}

export default ProfileModal;
