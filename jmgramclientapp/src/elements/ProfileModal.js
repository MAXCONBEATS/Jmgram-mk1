import React from 'react';
import UserProfile from './UserProfile';
import { updateContactName } from '../controllers/ContactController';

function ProfileModal({ userId, onClose, onContactDeleted }) {
  return (
    <UserProfile 
      userId={userId} 
      onClose={onClose} 
      onContactDeleted={onContactDeleted}
      updateContactName={updateContactName}
    />
  );
}

export default ProfileModal;