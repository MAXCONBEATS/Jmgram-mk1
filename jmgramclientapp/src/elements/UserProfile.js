import React, { useState, useEffect } from 'react';
import { UserController } from '../controllers/UserController';
import { deleteContact, getContactList } from '../controllers/ContactController';
import '../css/UserProfile.css';

const UserProfile = ({ 
  userId, 
  onClose, 
  onContactDeleted, 
  updateContactName 
}) => {
  const [profile, setProfile] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [contactNameEditMode, setContactNameEditMode] = useState(false);
  const [newContactName, setNewContactName] = useState('');
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    bio: ''
  });

  useEffect(() => {
    const loadProfile = async () => {
      try {
        const data = await UserController.getProfile(userId);
        console.log('Loaded profile:', data);
        setProfile(data);
        setFormData({
          firstName: data.firstName || '',
          lastName: data.lastName || '',
          bio: data.bio || ''
        });
        setNewContactName(`${data.firstName || ''} ${data.lastName || ''}`.trim());
      } catch (error) {
        console.error('Failed to load profile:', error);
      }
    };
    loadProfile();
  }, [userId]);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await UserController.updateProfile(formData);
      setProfile(prev => ({ ...prev, ...formData }));
      setEditMode(false);
    } catch (error) {
      console.error('Update failed:', error);
    }
  };

  const handleEditContactName = () => {
    setContactNameEditMode(true);
  };

  const handleContactNameSubmit = async (e) => {
    e.preventDefault();
    try {
      await updateContactName(userId, newContactName);
      alert('Имя контакта успешно обновлено!');
      setContactNameEditMode(false);
      setProfile(prev => ({ 
        ...prev, 
        displayName: newContactName 
      }));
    } catch (error) {
      console.error('Ошибка при обновлении имени контакта:', error);
      alert('Не удалось обновить имя контакта.');
    }
  };

  const handleCancelContactNameEdit = () => {
    setContactNameEditMode(false);
    setNewContactName(`${profile.firstName || ''} ${profile.lastName || ''}`.trim());
  };

  if (!profile) return <div className="user-profile-loading">Loading...</div>;

  const isOwnProfile = userId === localStorage.getItem('UserId');

  const handleDeleteContact = async () => {
    if (window.confirm('Вы уверены, что хотите удалить этот контакт?')) {
      try {
        await deleteContact(userId);
        alert('Контакт успешно удален.');
        if (onContactDeleted) {
          await onContactDeleted();
        }
        onClose();
      } catch (error) {
        console.error('Ошибка при удалении контакта:', error);
        alert('Не удалось удалить контакт.');
      }
    }
  };

  return (
    <div className="user-profile-overlay">
      <div className="user-profile-container">
        <button className="close-btn" onClick={onClose}>×</button>

        {contactNameEditMode && !isOwnProfile && (
          <div className="contact-name-edit-form">
            <h3>Изменить имя контакта</h3>
            <form onSubmit={handleContactNameSubmit}>
              <div className="form-group">
                <label>Новое имя контакта:</label>
                <input
                  type="text"
                  value={newContactName}
                  onChange={(e) => setNewContactName(e.target.value)}
                  placeholder="Введите новое имя контакта"
                  required
                />
              </div>
              <div className="form-actions">
                <button type="submit" className="save-btn">
                  Сохранить
                </button>
                <button 
                  type="button" 
                  className="cancel-btn"
                  onClick={handleCancelContactNameEdit}
                >
                  Отмена
                </button>
              </div>
            </form>
          </div>
        )}

        {editMode ? (
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Имя:</label>
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
              />
            </div>

            <div className="form-group">
              <label>Фамилия:</label>
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
              />
            </div>

            <div className="form-group">
              <label>О себе:</label>
              <textarea
                name="bio"
                value={formData.bio}
                onChange={handleChange}
                rows="4"
              />
            </div>

            <div className="form-actions">
              <button type="submit" className="save-btn">Сохранить</button>
              <button 
                type="button" 
                className="cancel-btn"
                onClick={() => setEditMode(false)}
              >
                Отмена
              </button>
            </div>
          </form>
        ) : (
          <div className="profile-info">
            <h2>
              {profile.displayName || `${profile.firstName} ${profile.lastName}`}
              {!isOwnProfile && profile.displayName && (
                <small>
                  (Настоящее имя: {profile.firstName} {profile.lastName})
                </small>
              )}
            </h2>
            {profile.bio && <p className="bio-text">{profile.bio}</p>}
            <p className="last-seen">Был в сети: {new Date(profile.lastSeen).toLocaleString()}</p>
            
            {isOwnProfile ? (
              <button onClick={() => setEditMode(true)} className="edit-btn">
                <i className="bi bi-pencil-square"></i> Редактировать профиль
              </button>
            ) : (
              <div className="contact-actions">
                {!contactNameEditMode && (
                  <button 
                    onClick={handleEditContactName}
                    className="edit-contact-name-btn"
                  >
                    Изменить имя контакта
                  </button>
                )}
                <button onClick={handleDeleteContact} className="delete-btn">
                  Удалить контакт
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default UserProfile;